using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

using Newtonsoft.Json.Serialization;

namespace FenalyticsScripts
{
	namespace Serialization
	{
		[Serializable]
		public class FenalyticsEvent
		{
			public string name;
			public Dictionary<string, object> data;
			public string session_id;
			public string start_at;
			public string root_id;

			public FenalyticsEvent (string _name, Dictionary<string, object>  _data, string _start_at, string _root_id = null)
			{
				name = _name;
				data = _data;
				start_at = _start_at;
			}
		}

		[Serializable]
		public class FenalyticsEndAt
		{
			public string end_at;

			public FenalyticsEndAt (string _end_at)
			{
				end_at = _end_at;
			}
		}

		[Serializable]
		public class FenalyticsBuild
		{
			public string project_id;
			public string build_version;
			public string build_hash;
			public string engine_version;

			public FenalyticsBuild (string _project_id, string _build_version, string _build_hash, string _engine_version)
			{
				project_id = _project_id;
				build_version = _build_version;
				build_hash = _build_hash;
				engine_version = _engine_version;
			}
		}

		[Serializable]
		public class FenalyticsSession
		{
			public string name;
			public string device_id;
			public string account_id;
			public string build_id;
			public string location_id;
			public string start_at;
			public string root_id;

			public string prev_id;

			public FenalyticsSession (string _name, string _start_at, string _device_id = null, string _account_id = null, string _build_id = null, string _prev_id = null, string _root_id = null)
			{
				name = _name;
				device_id = _device_id;
				account_id = _account_id;
				build_id = _build_id;
				start_at = _start_at;
				prev_id = _prev_id;
				root_id = _root_id;
			}
		}


		[Serializable]
		public class FenalyticsDevice
		{
			public string _id;
			public string name;
			public string model;
			public string type;
			public int ram;
			public FenalyticsGraphicsDevice gpu;
			public FenalyticsCentalDevice cpu;
			public FenalyticsDisplay display;
			public int display_count;

			public string platform;
			public string os;
			public string os_lang;
			public string os_family;

			public FenalyticsDevice (string __id, string _name, string _model, string _type, int _ram, FenalyticsGraphicsDevice _graphics_device,
			                         FenalyticsCentalDevice _central_device, FenalyticsDisplay _display, int _display_count,
			                         string _platform, string _os, string _os_family, string _os_lang)
			{
				_id = __id;
				name = _name;
				model = _model;
				type = _type;
				ram = _ram;
				os_lang = _os_lang;
				gpu = _graphics_device;
				cpu = _central_device;
				display = _display;
				display_count = _display_count;
				platform = _platform;
				os = _os;
				os_family = _os_family;
			}
		}

		[Serializable]
		public class FenalyticsGraphicsDevice
		{
			public string name;
			public string type;
			public string vendor;
			public int memory_size;

			public FenalyticsGraphicsDevice (string _name, string _type, string _vendor, int _memory_size)
			{
				name = _name;
				type = _type;
				vendor = _vendor;
				memory_size = _memory_size;
			}
		}

		[Serializable]
		public class FenalyticsCentalDevice
		{
			public string name;
			public int frequency;
			public int count;

			public FenalyticsCentalDevice (string _name, int _frequency, int _count)
			{
				name = _name;
				frequency = _frequency;
				count = _count;
			}
		}

		[Serializable]
		public class FenalyticsDisplay
		{
			public float[] rendering_size;
			public float[] screen_size;
			public bool fullscreen;
			public int refresh_rate;
			public string orientation;

			public FenalyticsDisplay (Vector2 _rendering_size, Vector2 _screen_size, bool _fullscreen, int _refresh_rate, string _orientation)
			{
				VectorUtils.SerializeVector2 (_rendering_size, out rendering_size);
				VectorUtils.SerializeVector2 (_screen_size, out screen_size);
				fullscreen = _fullscreen;
				refresh_rate = _refresh_rate;
				orientation = _orientation;
			}
		}

		public static class VectorUtils
		{
			public static void SerializeVector2 (Vector2 vector2, out float[] _vector2)
			{
				_vector2 = new float[2]{ vector2 [0], vector2 [1] };
			}

			public static void SerializeVector3 (Vector3 vector3, out float[] _vector3)
			{	
				_vector3 = new float[3]{ vector3 [0], vector3 [1], vector3 [2] };
			}
		}
	}
}