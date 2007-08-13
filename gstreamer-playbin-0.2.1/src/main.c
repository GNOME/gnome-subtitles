/*

	Copyright (c)  Goran Sterjov

    This file is part of the GStreamer Playbin Wrapper.
    Derived from the Dissent Project.

    GStreamer Playbin Wrapper is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    The Dissent Project is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with the Dissent Project; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/



#include <gst/gst.h>
#include <gst/interfaces/xoverlay.h>

typedef struct gstPlay gstPlay;

// callbacks for the binding
typedef void (* eosCallback) (gstPlay *play);
typedef void (* errorCallback) (gstPlay *play, const gchar * error, const gchar *debug);
typedef void (* bufferCallback) (gstPlay *play, gint progress);


// a simple structure for the created playbin
struct gstPlay {
    GstElement *element;
    gulong xid;
	GstXOverlay *overlay;
	
	eosCallback eos_cb;
	errorCallback error_cb;
	bufferCallback buffer_cb;
};


static GstBusSyncReply
gst_sync_watch (GstBus *bus, GstMessage *message, gpointer data)
{
	gstPlay *play = (gstPlay *)data;
	if (play == NULL) return FALSE;
	
	if (GST_MESSAGE_TYPE (message) == GST_MESSAGE_ELEMENT) {
		if (gst_structure_has_name (message->structure, "prepare-xwindow-id")) {
			play->overlay = GST_X_OVERLAY (GST_MESSAGE_SRC (message));
			gst_x_overlay_set_xwindow_id (play->overlay, play->xid);
		}
	}
	return TRUE;
}


static GstBusSyncReply
gst_async_watch(GstBus *bus, GstMessage *message, gpointer data)
{
    gstPlay *play = (gstPlay *)data;
	if (play == NULL) return FALSE;
	
	switch (GST_MESSAGE_TYPE (message)) {
		case GST_MESSAGE_ERROR:
		{
			if(play->error_cb != NULL) {
				GError *error; gchar *debug;
				gst_message_parse_error (message, &error, &debug);
				play->error_cb (play, error->message, debug);
				g_error_free (error);
				g_free (debug);
			}
			break;
		}
		case GST_MESSAGE_EOS:
            if(play->eos_cb != NULL)
				play->eos_cb(play);
            break;
		case GST_MESSAGE_BUFFERING: {
            const GstStructure *buffer;
            gint prog = 0;
            
            buffer = gst_message_get_structure (message);
            if(gst_structure_get_int (buffer, "buffer-percent", &prog))
                if(play->buffer_cb != NULL)
					play->buffer_cb(play, prog);
			break;
        }
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
		gst_sync_watch, play);
	gst_bus_add_watch (gst_pipeline_get_bus(GST_PIPELINE(play->element)), 
		gst_async_watch, play);
	
	return play;
}

// releases any references to gstreamer
void gst_binding_deinit (gstPlay *play) {
	if (isValid (play)) {
		gst_element_set_state (play->element, GST_STATE_NULL);
		gst_object_unref (GST_OBJECT (play->element));
		play->element = NULL;
	}
}


// loads a uri into the pipeline
void gst_binding_load (gstPlay *play, char *uri) {
	if (isValid (play))
		g_object_set (G_OBJECT (play->element), "uri", uri, NULL);
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
	if (isValid (play))
		gst_element_set_state (play->element, GST_STATE_NULL);
}


// retrieves the duration of the media file
guint64 gst_binding_get_duration (gstPlay *play) {
	if (!isValid (play)) return 0;
	
	GstFormat format = GST_FORMAT_TIME;
	gint64 duration;
	if(gst_element_query_duration (play->element, &format, &duration))
		return duration / GST_MSECOND;
	return 0;
}

// retrieves the position of the media file
guint64 gst_binding_get_position (gstPlay *play) {
	if (!isValid (play)) return 0;
	
	GstFormat format = GST_FORMAT_TIME;
    gint64 position;
	if(gst_element_query_position (play->element, &format, &position))
		return position / GST_MSECOND;
	return 0;
}

// set the position of the media file
void gst_binding_set_position (gstPlay *play, gint64 time_sec) {
	if (!isValid (play)) return;
	
	gst_element_seek (play->element, 1.0, GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH,
					  GST_SEEK_TYPE_SET, time_sec * GST_MSECOND,
					  GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
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
	gint cur_video;
	g_object_get (play->element, "current-video", &cur_video, NULL);
	if (cur_video == -1) return FALSE;
	else return TRUE;
}


void gst_binding_set_xid (gstPlay *play, gulong xid) {
	if (play != NULL)
		gst_x_overlay_set_xwindow_id (play->overlay, xid);
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
