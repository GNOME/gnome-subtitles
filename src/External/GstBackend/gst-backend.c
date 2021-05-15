/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2021 Pedro Castro
 *
 * Gnome Subtitles is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Gnome Subtitles is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA	02110-1301	USA
 */

#include <gst/gst.h>
#include <gst/pbutils/gstdiscoverer.h>


/* Typedefs */

typedef struct GstBackendMediaInfo GstBackendMediaInfo;
typedef struct GstBackend GstBackend;


/* Enums */

enum GstBackendErrorType { GST_BACKEND_ERR_GSTREAMER, GST_BACKEND_ERR_NO_MEDIA_INFO, GST_BACKEND_ERR_NO_VIDEO_OR_AUDIO };


/* Callbacks */

typedef void (*ErrorCallback)(enum GstBackendErrorType type, const gchar *user_message, const gchar *debug_message);
typedef void (*BasicCallback)();
typedef void (*LoadCompleteCallback)(GstBackendMediaInfo *media_info);


/* Structures */

struct GstBackendMediaInfo { 
	/* video and audio */
	gint64 duration;

	/* video */
	gboolean has_video;
	gint width;
	gint height;
	gfloat aspect_ratio;
	gfloat frame_rate;

	/* audio */
	gboolean has_audio;
};

struct GstBackend {
	GstElement *element; //playbin
	GstElement *video_element; //gtksink

	GstBackendMediaInfo *media_info;

	ErrorCallback error_callback;
	LoadCompleteCallback load_complete_callback;
	BasicCallback eos_reached_callback;
};


/* Static Functions */

static GstBackend *gst_backend_new() {
	GstBackend *backend = g_new0(GstBackend, 1);
	return backend;
}

static GstBackendMediaInfo *gst_backend_media_info_new() {
	GstBackendMediaInfo *media_info = g_new0(GstBackendMediaInfo, 1);

	/* video and audio */
	media_info->duration = -1;

	/* video */
	media_info->has_video = FALSE;
	media_info->width = -1;
	media_info->height = -1;
	media_info->aspect_ratio = -1;
	media_info->frame_rate = -1;

	/* audio */
	media_info->has_audio = FALSE;

	return media_info;
}

static void throw_error(GstBackend *backend, enum GstBackendErrorType type, const gchar *user_message, const gchar *debug_message) {
	if (backend->error_callback != NULL) {
		backend->error_callback(type, user_message, debug_message);
	}
}

/*
 * Blocks until a pending state change (if applicable) completes.
 */
static void wait_for_state_change_to_complete(GstBackend *backend) {
	gst_element_get_state(backend->element, NULL, NULL, GST_CLOCK_TIME_NONE);
}

static gboolean media_has_video(GstBackend *backend) {
	gint current_video;
	g_object_get(backend->element, "current-video", &current_video, NULL);
	return current_video != -1;
}

static gboolean media_has_audio(GstBackend *backend) {
	gint current_audio;
	g_object_get(backend->element, "current-audio", &current_audio, NULL);
	return current_audio != -1;
}

static void load_media_info_video(GstBackend *backend) {
	GstElement *video_sink;
	g_object_get(G_OBJECT(backend->element), "video-sink", &video_sink, NULL);

	if (video_sink == NULL) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video sink");
		return;
	}

	GstPad *video_pad = gst_element_get_static_pad(GST_ELEMENT(video_sink), "sink");
	if (video_pad == NULL) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video pad");
		return;
	}

	GstCaps *caps =  gst_pad_get_current_caps(video_pad);
	if (caps == NULL) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video caps");
		return;
	}

	guint caps_count = gst_caps_get_size(caps);
	gboolean got_width = FALSE, got_height = FALSE, got_frame_rate = FALSE;
	for (guint i = 0; i < caps_count; i++) {
		GstStructure *structure = gst_caps_get_structure(caps, i);

		/* Ignore if not video */
		const gchar *name = gst_structure_get_name(structure);
		if (!g_str_has_prefix(name, "video")) {
			continue;
		}

		/* Get the values */
		if (!got_width) {
			got_width = gst_structure_get_int(structure, "width", &backend->media_info->width);
		}
		if (!got_height) {
			got_height = gst_structure_get_int(structure, "height", &backend->media_info->height);
		}
		if (!got_frame_rate) {
			gint numerator, denominator;
			got_frame_rate = gst_structure_get_fraction(structure, "framerate", &numerator, &denominator);
			if (got_frame_rate) {
				backend->media_info->frame_rate = (float)numerator / (float)denominator;
			}
		}
		
		if (got_width && got_height && got_frame_rate) {
			break;
		}
	}

	gst_caps_unref(caps);

	if (!got_width) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video width");
		return;
	}

	if (!got_height) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video height");
		return;
	}

	if (!got_frame_rate) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to obtain the video frame rate");
		return;
	}

	backend->media_info->aspect_ratio = (float)backend->media_info->width / (float)backend->media_info->height;
}

static void on_discovered(GstDiscoverer *discoverer, GstDiscovererInfo *info, GError *error, gpointer data) {
	GstBackend *backend = (GstBackend *)data;
	if (backend == NULL) {
		g_debug("on_discovered user_data is NULL. Unable to continue."); //can't call backend->error_callback because backend is null
		return;
	}

	GstClockTime duration = gst_discoverer_info_get_duration(info);
	backend->media_info->duration = duration / GST_MSECOND;

	backend->load_complete_callback(backend->media_info);
}

static void load_media_info_duration(GstBackend *backend) {

	gint64 duration;
	if (gst_element_query_duration(backend->element, GST_FORMAT_TIME, &duration)) {
		backend->media_info->duration = duration / GST_MSECOND;
		backend->load_complete_callback(backend->media_info);
		return;
	}

	/* Usually we can query the duration above. However, there are
	 * some files where this doesn't happen (for example, some audio files
	 * where the duration is only computed when starting to play the file).
	 * In those cases, the GstDiscoverer is used below.
	 */
	g_debug("Unable to query the media duration. Using the discoverer.");
	gchararray uri;
	g_object_get(G_OBJECT(backend->element), "current-uri", &uri, NULL);

	GstDiscoverer *discoverer = gst_discoverer_new(GST_SECOND, NULL);
	if (discoverer == NULL) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Unable to get the gstreamer discoverer");
		return;
	}

	g_signal_connect(discoverer, "discovered", G_CALLBACK(on_discovered), backend);
	gst_discoverer_start(discoverer);
	
	if (!gst_discoverer_discover_uri_async(discoverer, uri)) {
		throw_error(backend, GST_BACKEND_ERR_NO_MEDIA_INFO, NULL, "Failed to start the discoverer");
		return;
	}
}

static void load_media_info(GstBackend *backend) {

	backend->media_info = gst_backend_media_info_new();

	/* Check for video and audio */
	backend->media_info->has_video = media_has_video(backend);
	backend->media_info->has_audio = media_has_audio(backend);

	if (!backend->media_info->has_video && !backend->media_info->has_audio) {
		throw_error(backend, GST_BACKEND_ERR_NO_VIDEO_OR_AUDIO, NULL, NULL);
		return;
	}

	if (backend->media_info->has_video) {
		load_media_info_video(backend);
	}

	load_media_info_duration(backend);
}

static gboolean on_gst_message(GstBus *bus, GstMessage *message, gpointer data) {

	GstBackend *backend = (GstBackend *)data;
	if (backend == NULL) {
		g_debug("on_gst_message user_data is NULL. Unable to continue."); //can't call backend->error_callback because backend is null
		return FALSE;
	}

	switch (GST_MESSAGE_TYPE(message)) {

		/* ASYNC_DONE can be emitted many times (example: when returning back from
		 * pause to play). If the info is already loaded, don't do anything.
		 */
		case GST_MESSAGE_ASYNC_DONE: {
			if (!backend->media_info) {
				load_media_info(backend);
			}
			break;
		}

		/*case GST_MESSAGE_STATE_CHANGED: {
			GstState old_state, new_state, pending_state;
			gst_message_parse_state_changed(message, &old_state, &new_state, &pending_state);
			g_print("on_gst_message: STATE CHANGED: old=%s, new(current)=%s, pending(target)=%s\n",
				gst_element_state_get_name(old_state), gst_element_state_get_name(new_state), gst_element_state_get_name(pending_state));
			break;
		}*/

		case GST_MESSAGE_ERROR: {
			if (backend->error_callback != NULL) {
				GError *error;
				gchar *debug;
				gst_message_parse_error(message, &error, &debug);

				throw_error(backend, GST_BACKEND_ERR_GSTREAMER, error->message, debug);

				g_error_free(error);
				g_free(debug);
			}
			break;
		}

		// the media file finished playing
		case GST_MESSAGE_EOS: {
			if (backend->eos_reached_callback != NULL) {
				backend->eos_reached_callback();
			}
			break;
		}

		default:
			break;
	}
	return TRUE;
}


/* 'Public' functions */

GstBackend *gst_backend_init(ErrorCallback error_callback, LoadCompleteCallback load_complete_callback,
		BasicCallback eos_reached_callback) {

	GstBackend *backend = gst_backend_new();

	backend->error_callback = error_callback;
	backend->load_complete_callback = load_complete_callback;
	backend->eos_reached_callback = eos_reached_callback;

	gst_init(NULL, NULL);

	backend->element = gst_element_factory_make("playbin", "play");
	if (backend->element == NULL) {
		g_debug("gst_element_factory_make returned a null playbin element");
		return NULL;
	}

	backend->video_element = gst_element_factory_make("gtksink", "gtksink");
	if (backend->video_element == NULL) {
		g_debug("gst_element_factory_make returned a null gtksink element");
		return NULL;
	}

	g_object_set(G_OBJECT(backend->element), "video-sink", backend->video_element, NULL);

	gst_bus_add_watch(gst_pipeline_get_bus(GST_PIPELINE(backend->element)), on_gst_message, backend);

	return backend;
}

gboolean gst_backend_load(GstBackend *backend, char *uri) {
	g_object_set(G_OBJECT(backend->element), "uri", uri, NULL);
	return (gst_element_set_state(backend->element, GST_STATE_PAUSED) != GST_STATE_CHANGE_FAILURE);
}

void gst_backend_play(GstBackend *backend) {
	gst_element_set_state(backend->element, GST_STATE_PLAYING);
}

void gst_backend_pause(GstBackend *backend) {
	gst_element_set_state(backend->element, GST_STATE_PAUSED);
}

void gst_backend_unload(GstBackend *backend) {
	gst_element_set_state(backend->element, GST_STATE_NULL);
	
	if (backend->element != NULL) {
		gst_object_unref(GST_OBJECT(backend->element));
		backend->element = NULL;
	}

	g_free(backend->media_info);
	backend->media_info = NULL;

	g_free(backend);
	backend = NULL;
}

gint64 gst_backend_get_position(GstBackend *backend) {
	gint64 position;
	if (gst_element_query_position(backend->element, GST_FORMAT_TIME, &position)) {
		return position / GST_MSECOND;
	}

	/* Query position wasn't successful. This is usually due to a pending state change,
	 * which happens for example after seeking, so we wait for the state change to complete
	 * and try again.
	 */
	wait_for_state_change_to_complete(backend);
	if (gst_element_query_position(backend->element, GST_FORMAT_TIME, &position)) {
		return position / GST_MSECOND;
	}

	/* If we're here, we're still unable to return the position after waiting for the state
	 * change to complete. Return -1. */
	g_debug("gst_backend_get_position unable to query the current position");
	return -1;
}

/*
 * time: in ms
 */
void gst_backend_seek(GstBackend *backend, gint64 time, gboolean is_absolute, float speed) {
	gint64 new_position = (is_absolute ? time : gst_backend_get_position(backend) + time);

	/* Note: gst_element_seek_simple can't be used here because it always resets speed to 1, 
	 * and we need to keep the current speed.
	 */
	gst_element_seek(backend->element, speed, GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH,
		GST_SEEK_TYPE_SET, new_position * GST_MSECOND, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
}

void gst_backend_set_speed(GstBackend *backend, float speed) {
	gst_element_seek(backend->element, speed, GST_FORMAT_TIME, GST_SEEK_FLAG_FLUSH,
		GST_SEEK_TYPE_SET, gst_backend_get_position(backend) * GST_MSECOND, GST_SEEK_TYPE_NONE, GST_CLOCK_TIME_NONE);
}

/* Gets the GtkWidget inside the gtksink video element */
void *gst_backend_get_video_widget(GstBackend *backend) {
	void *widget;
	g_object_get(backend->video_element, "widget", &widget, NULL);
	return widget;
}
