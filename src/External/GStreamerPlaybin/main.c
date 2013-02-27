/*

	Copyright (C) 2007 Goran Sterjov, Pedro Castro
	Copyright (C) 2011 Pedro Castro

    This file is part of the GStreamer Playbin Wrapper.
    Derived from Fuse.

    GStreamer Playbin Wrapper is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    GStreamer Playbin Wrapper is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with GStreamer Playbin Wrapper; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/



#include <gst/gst.h>
#include <gst/video/videooverlay.h>
#include <gst/tag/tag.h> 
#include <string.h>



typedef struct gstPlay gstPlay;
typedef struct gstVideoInfo gstVideoInfo;
typedef struct gstTag gstTag;

// callbacks for the binding
typedef void (* eosCallback) ();
typedef void (* errorCallback) (const gchar *error, const gchar *debug);
typedef void (* bufferCallback) (gint progress);
typedef void (* infoCallback) (gstVideoInfo *video_info);
typedef void (* tagCallback) (gstTag *tag);


// a video info structure
struct gstVideoInfo {
	gint width;
	gint height;
	gfloat aspect_ratio;
	gfloat frame_rate;
	gboolean has_audio;
	gboolean has_video;
};


// a media tag structure
struct gstTag {
	gchar *disc_id;
	gchar *music_brainz_id;
	
	guint current_track;
	guint track_count;
	guint64 duration;
};

// a simple structure for the created playbin
struct gstPlay {
    GstElement *element;
    gulong xid;
	GstVideoOverlay *overlay;
	
	gchar *vis_name;
	
	eosCallback eos_cb;
	errorCallback error_cb;
	bufferCallback buffer_cb;
	infoCallback info_cb;
	tagCallback tag_cb;

	gstVideoInfo *video_info;
	gboolean info_loaded;
	
	gstTag *tag;
};


//Declarations
static void setup_vis (gstPlay *play);
gboolean gst_binding_load_video_info (gstPlay *play);
gboolean gst_binding_has_video (gstPlay *play);
gboolean gst_binding_has_audio (gstPlay *play);


static GstBusSyncReply
gst_sync_watch (GstBus *bus, GstMessage *message, gpointer data)
{
	gstPlay *play = (gstPlay *)data;
	if (play == NULL) return FALSE;
	
	if (GST_MESSAGE_TYPE (message) == GST_MESSAGE_ELEMENT) {
		if (gst_is_video_overlay_prepare_window_handle_message(message)) {
			play->overlay = GST_VIDEO_OVERLAY (GST_MESSAGE_SRC (message));
			gst_video_overlay_set_window_handle (play->overlay, play->xid);
		}
	}
	return TRUE;
}


static gboolean
gst_async_watch(GstBus *bus, GstMessage *message, gpointer data)
{
	gstPlay *play = (gstPlay *)data;
	if (play == NULL) return FALSE;
	
	switch (GST_MESSAGE_TYPE (message)) {
		
		// the pipeline state has changed
		case GST_MESSAGE_STATE_CHANGED:
		{
			GstState new_state;
			gst_message_parse_state_changed (message, NULL, &new_state, NULL);
			
			if (new_state == GST_STATE_PAUSED)
			{
				if (play->info_loaded == FALSE)
				{
					if (gst_binding_load_video_info (play))
					{
						play->info_loaded = TRUE;
						if(play->info_cb != NULL) {
							play->info_cb (play->video_info);
						}
					}
				}
			}
			
			break;
		}
		
		// and error occurred in the pipeline
		case GST_MESSAGE_ERROR:
		{
			if(play->error_cb != NULL) {
				GError *error; gchar *debug;
				gst_message_parse_error (message, &error, &debug);
				play->error_cb (error->message, debug);
				g_error_free (error);
				g_free (debug);
			}
			break;
		}
		
		// the media file finished playing
		case GST_MESSAGE_EOS:
        {
        	if(play->eos_cb != NULL)
				play->eos_cb();
            break;
        }
        
        // the media file is being buffered
		case GST_MESSAGE_BUFFERING:
		{
            const GstStructure *buffer;
            gint prog = 0;
            
            buffer = gst_message_get_structure (message);
            if(gst_structure_get_int (buffer, "buffer-percent", &prog))
                if(play->buffer_cb != NULL)
					play->buffer_cb(prog);
			break;
        }
        
         // the media file has a tag
		case GST_MESSAGE_TAG:
		{
			play->tag = g_new0 (gstTag, 1);
			
            GstTagList *tags;
            gst_message_parse_tag (message, &tags);
            
            guint64 duration;
            guint current_track;
            guint track_count;
            char *disc_id;
            char *music_brainz_id;
            
            
            // track number
            if (gst_tag_list_get_uint (tags, GST_TAG_TRACK_NUMBER, &current_track))
            	play->tag->current_track = current_track;
           	
           	// total tracks
           	if (gst_tag_list_get_uint (tags, GST_TAG_TRACK_COUNT, &track_count))
            	play->tag->track_count = track_count;
           	
           	// track duration
           	if (gst_tag_list_get_uint64 (tags, GST_TAG_DURATION, &duration))
           		play->tag->duration = duration;
            
            // track cddb disc id
            if (gst_tag_list_get_string (tags, GST_TAG_CDDA_CDDB_DISCID, &disc_id))
           		play->tag->disc_id = disc_id;
           	
           	// track music brainz disc id
           	if (gst_tag_list_get_string (tags, GST_TAG_CDDA_MUSICBRAINZ_DISCID, &music_brainz_id))
           		play->tag->music_brainz_id = music_brainz_id;
           	
           	if(play->tag_cb != NULL)
					play->tag_cb (play->tag);
           	
			break;
        }

        //By default, do nothing
        default:
        	break;
	}
	return TRUE;
}


gboolean isValid (gstPlay *play) {
	if (play != NULL)
		if (GST_IS_ELEMENT (play->element)) return TRUE;
	return FALSE;
}



// initiates gstreamer as a playbin pipeline
gstPlay *gst_binding_init (gulong xwin) {
	gstPlay *play = g_new0 (gstPlay, 1);
	
	gst_init (NULL, NULL);
	play->element = gst_element_factory_make ("playbin", "play");
	if (play->element == NULL) return NULL;
	play->xid = xwin;
	
	gst_bus_set_sync_handler (gst_pipeline_get_bus(GST_PIPELINE(play->element)), 
		gst_sync_watch, play, NULL);
	gst_bus_add_watch (gst_pipeline_get_bus(GST_PIPELINE(play->element)), 
		gst_async_watch, play);
	
	return play;
}



// releases any references to gstreamer
void gst_binding_deinit (gstPlay *play) {
	if (isValid (play)) {
		gst_element_set_state (play->element, GST_STATE_NULL);
		
		if (play->element != NULL)
		{
    		gst_object_unref (GST_OBJECT (play->element));
    		play->element = NULL;
		}
		
		g_free (play->vis_name);
		play->vis_name = NULL;
		
		g_free (play->video_info);
		play->video_info = NULL;
		
		if (play->tag != NULL)
		{
    		g_free (play->tag->disc_id);
    		play->tag->disc_id = NULL;
    		
    		g_free (play->tag->music_brainz_id);
    		play->tag->music_brainz_id = NULL;
    		
    		g_free (play->tag);
    		play->tag = NULL;
		}
		
		g_free (play);
		play = NULL;
	}
}


// loads a uri into the pipeline
gboolean gst_binding_load (gstPlay *play, char *uri) {
	if (isValid (play))
	{
		g_object_set (G_OBJECT (play->element), "uri", uri, NULL);
		if (gst_element_set_state (play->element, GST_STATE_PAUSED) != GST_STATE_CHANGE_FAILURE)
		    return TRUE;
	}
	
	return FALSE;
}

// plays the specified uri in the pipeline
void gst_binding_play (gstPlay *play) {
	if (isValid (play))
		gst_element_set_state (play->element, GST_STATE_PLAYING);
}

// pauses the specified uri in the pipeline
void gst_binding_pause (gstPlay *play) {
	if (isValid (play))
		gst_element_set_state (play->element, GST_STATE_PAUSED);
}

// unloads the media in the pipeline
void gst_binding_unload (gstPlay *play) {
	if (isValid (play)) {
		gst_element_set_state (play->element, GST_STATE_NULL);

		g_free (play->video_info);
		play->video_info = NULL;
		play->info_loaded = FALSE;
	}
}


// retrieves the duration of the media file
guint64 gst_binding_get_duration (gstPlay *play) {
	if (!isValid (play)) return 0;
	
	gint64 duration;
	if(gst_element_query_duration (play->element, GST_FORMAT_TIME, &duration))
		return duration / GST_MSECOND;
	return 0;
}

// retrieves the position of the media file
guint64 gst_binding_get_position (gstPlay *play) {
	if (!isValid (play)) return 0;
	
    gint64 position;
	if(gst_element_query_position (play->element, GST_FORMAT_TIME, &position))
		return position / GST_MSECOND;
	return 0;
}

// set the position of the media file
void gst_binding_set_position (gstPlay *play, gint64 time_sec, float speed) {
	if (!isValid (play)) return;
	
	gst_element_seek (play->element, speed, GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH,
					  GST_SEEK_TYPE_SET, time_sec * GST_MSECOND,
					  GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
}


// set the position of the media file
void gst_binding_set_track (gstPlay *play, gint64 track_number, float speed) {
	if (!isValid (play)) return;
	
	gst_element_seek (play->element, speed, gst_format_get_by_nick ("track"),
	      GST_SEEK_FLAG_FLUSH, GST_SEEK_TYPE_SET, track_number - 1,
	      GST_SEEK_TYPE_NONE, -1);
}


// sets the volume
void gst_binding_set_volume (gstPlay *play, gint vol) {
	if (!isValid (play)) return;
	
	gdouble volume;
    volume = CLAMP(vol, 0, 100) / 100.0;
	g_object_set(G_OBJECT(play->element), "volume", volume, NULL);
}

// gets the volume
gint gst_binding_get_volume (gstPlay *play) {
	if (!isValid (play)) return 0;
	
    gdouble vol = 0.0;
    g_object_get(play->element, "volume", &vol, NULL);
    return (gint)(vol * 100.0);
}

gboolean gst_binding_has_video (gstPlay *play) {
	if (!isValid (play)) return FALSE;
	
	gint cur_video;
	g_object_get (play->element, "current-video", &cur_video, NULL);
	if (cur_video == -1) return FALSE;
	else return TRUE;
}

gboolean gst_binding_has_audio (gstPlay *play) {
	if (!isValid (play)) return FALSE;
	
	gint cur_audio;
	g_object_get (play->element, "current-audio", &cur_audio, NULL);
	if (cur_audio == -1) return FALSE;
	else return TRUE;
}

//returns the tag information
gstTag *gst_binding_get_tag (gstPlay *play) {
	if (isValid (play))
		return play->tag;
	else return NULL;
}



//returns the video info without loading it again
gstVideoInfo *gst_binding_get_video_info (gstPlay *play) {
	if (isValid (play))
		return play->video_info;
	else return NULL;
}


//retrieves video information, or NULL if it's not available
gboolean gst_binding_load_video_info (gstPlay *play) {
	GstElement *audio_sink;
	GstElement *video_sink;

	if (!isValid (play)) return FALSE;
	
	g_object_get (G_OBJECT (play->element), "audio-sink", &audio_sink,
					       	"video-sink", &video_sink,
						NULL);
	
	/* Initialize video info structure */
	if (play->video_info == NULL) {
		play->video_info = g_new0 (gstVideoInfo, 1);
	}
	
	/* Check if audio or video is available */
	play->video_info->has_video = gst_binding_has_video(play);
	play->video_info->has_audio = gst_binding_has_audio(play);
	
	/* Only check for video details if a video stream is present */
	if (!play->video_info->has_video)
		return play->video_info->has_audio;
	
	if (video_sink != NULL) {
		GstPad *video_pad;
		video_pad = gst_element_get_static_pad (GST_ELEMENT (video_sink), "sink");
		if (video_pad != NULL) {
			GstCaps *caps;
			if ((caps = gst_pad_get_current_caps (video_pad)) != NULL) {
				gint caps_count = gst_caps_get_size (caps), caps_index;
				GstStructure *caps_struct;
				const GValue *caps_value;
				gint caps_width = -1, caps_height = -1;
				gfloat caps_frame_rate = -1;
				for (caps_index = 0; caps_index < caps_count; caps_index++) {
					caps_struct = gst_caps_get_structure (caps, caps_index);

					/* Check if mime type is video */
					const gchar *mime_type;
					mime_type = gst_structure_get_name (caps_struct);
					if ((!mime_type) || (g_ascii_strncasecmp(mime_type, "video", 5)))
						continue;

					/* Look for width */
					caps_value = gst_structure_get_value (caps_struct, "width");
					if (caps_value && (G_VALUE_TYPE (caps_value) == G_TYPE_INT))
						caps_width = g_value_get_int(caps_value);

					/* Look for height */
					caps_value = gst_structure_get_value (caps_struct, "height");
					if (caps_value && (G_VALUE_TYPE (caps_value) == G_TYPE_INT))
						caps_height = g_value_get_int(caps_value);

					/* Look for frame rate */
					caps_value = gst_structure_get_value (caps_struct, "framerate");
					if (caps_value && (G_VALUE_TYPE (caps_value) == GST_TYPE_FRACTION)) {
						int num = caps_value->data[0].v_int, den = caps_value->data[1].v_int;
						caps_frame_rate = (float)num/den;
					}
				}
				gst_caps_unref (caps);
				if ((caps_width != -1) && (caps_height != -1) && (caps_frame_rate != -1)) {
					play->video_info->width = caps_width;
					play->video_info->height = caps_height;
					play->video_info->aspect_ratio = ((float)caps_width)/((float)caps_height);
					play->video_info->frame_rate = caps_frame_rate;
					return TRUE;
				}
			}
		}
	}
	return FALSE;
}

void gst_binding_set_xid (gstPlay *play, gulong xid) {
	if (play == NULL)
		return;
	
	play->xid = xid;
	if (play->overlay != NULL && GST_IS_VIDEO_OVERLAY (play->overlay))
		gst_video_overlay_set_window_handle (play->overlay, xid);
}

void gst_binding_set_eos_cb(gstPlay *play, eosCallback cb) {
    if (play != NULL) play->eos_cb = cb;
}
void gst_binding_set_error_cb(gstPlay *play, errorCallback cb) {
	if (play != NULL) play->error_cb = cb;
}
void gst_binding_set_buffer_cb(gstPlay *play, bufferCallback cb) {
    if (play != NULL) play->buffer_cb = cb;
}
void gst_binding_set_info_cb(gstPlay *play, infoCallback cb) {
    if (play != NULL) play->info_cb = cb;
}
void gst_binding_set_tag_cb(gstPlay *play, tagCallback cb) {
    if (play != NULL) play->tag_cb = cb;
}






gboolean
filter_features (GstPluginFeature *feature, gpointer data)
{
  GstElementFactory *f;

  if (!GST_IS_ELEMENT_FACTORY (feature))
    return FALSE;
  f = GST_ELEMENT_FACTORY (feature);
  if (!g_strrstr (gst_element_factory_get_klass (f), "Visualization"))
    return FALSE;

  return TRUE;
}



GList *
get_visualization_features (void)
{
  return gst_registry_feature_filter (gst_registry_get (),
      filter_features, FALSE, NULL);
}





//finds the visualisation factory
GstElementFactory *
setup_vis_find_factory (const gchar *vis_name)
{
  GstElementFactory *fac = NULL;
  GList *l, *features;

  features = get_visualization_features ();

  /* find element factory using long name */
  for (l = features; l != NULL; l = l->next) {
    GstElementFactory *f = GST_ELEMENT_FACTORY (l->data);
    
    
    //long name
    if (f && strcmp (vis_name, gst_element_factory_get_longname (f)) == 0) {
      fac = f;
      goto done;
    }
    
    //short name
    else if (f && strcmp (vis_name, GST_OBJECT_NAME (f)) == 0) {
      fac = f;
      goto done;
    }
    
  }
  

done:
  g_list_free (features);
  return fac;
}








// setups audio visualization
// a modified version of totem's bacon video widget
static void
setup_vis (gstPlay *play)
{
	if (play->xid == 0)
		return;
	
	GstElement *vis_bin = NULL;
	GstElement *vis_element = NULL;
	GstElement *vis_capsfilter = NULL;
	GstPad *pad = NULL;
	GstElementFactory *fac = NULL;
	
	
	fac = setup_vis_find_factory (play->vis_name);
	if (fac == NULL)
		goto beach; //cant find the visualisation
	
	
	vis_element = gst_element_factory_create (fac, "vis_element");
	if (!GST_IS_ELEMENT (vis_element))
		goto beach; //cant create visualisation element
	
	
	
	vis_capsfilter = gst_element_factory_make ("capsfilter", "vis_capsfilter");
	if (!GST_IS_ELEMENT (vis_capsfilter))
	{
		gst_object_unref (vis_element);
		goto beach; //cant create visualisation capsfilter element
	}
	
	
	vis_bin = gst_bin_new ("vis_bin");
	if (!GST_IS_ELEMENT (vis_bin))
	{
		gst_object_unref (vis_element);
		gst_object_unref (vis_capsfilter);
		goto beach; //cant create visualisation bin
	}
	
	
	gst_bin_add_many (GST_BIN (vis_bin), vis_element, vis_capsfilter, NULL);
	
	// sink ghostpad
	pad = gst_element_get_static_pad (vis_element, "sink");
	gst_element_add_pad (vis_bin, gst_ghost_pad_new ("sink", pad));
	gst_object_unref (pad);
	
	
	// source ghostpad, link with vis_element
	pad = gst_element_get_static_pad (vis_capsfilter, "src");
	gst_element_add_pad (vis_bin, gst_ghost_pad_new ("src", pad));
	gst_element_link_pads (vis_element, "src", vis_capsfilter, "sink");
	gst_object_unref (pad);
	
beach:
	g_object_set (play->element, "vis-plugin", vis_bin, NULL);
	
	return;
}






void
add_longname (GstElementFactory *f, GList ** to)
{
  *to = g_list_append (*to, (gchar *) gst_element_factory_get_longname (f));
}




void
gst_binding_set_visual (gstPlay *play, const gchar *vis_name)
{
	play->vis_name = g_strdup (vis_name);
	setup_vis (play);
}


GList *
gst_binding_get_visuals_list (gstPlay *play)
{
  GList *features, *names = NULL;

  if (!isValid (play)) return NULL;


  features = get_visualization_features ();
  g_list_foreach (features, (GFunc) add_longname, &names);
  g_list_free (features);

  return names;
}
