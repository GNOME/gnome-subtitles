/*
 * This file is part of Gnome Subtitles.
 * Copyright (C) 2020 Pedro Castro
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
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace External.Enchant {

public class Enchant {
	private IntPtr lib;
	private IntPtr broker;
	private string version;
	
	/* Constants */
	
	private const string LIBENCHANT_2_SO = "libenchant-2.so.2"; //ver 2
	private const string LIBENCHANT_1_SO = "libenchant.so.1"; //ver 1
	private const string LIBENCHANT_SO = "libenchant.so"; //tentative fallback if the other two don't exist
	

	/* Delegates */
	private delegate void ProviderListHandler (string providerName, string providerDesc, string providerFile, IntPtr userdata);
	private delegate void LanguageListHandler (string langTag, string providerName, string providerDesc, string providerFile, IntPtr userdata);
	
	private delegate IntPtr EnchantBrokerInitDelegate ();
	private delegate void EnchantBrokerDescribeDelegate (IntPtr broker, ProviderListHandler cb, IntPtr userdata);
	private delegate void EnchantBrokerListDictsDelegate (IntPtr broker, LanguageListHandler cb, IntPtr userdata);
	private delegate void EnchantBrokerFreeDelegate (IntPtr broker);

	
	/* Public members */
	
	public string Version {
		get { return version; }
	}
	
	public void Open () {
		lib = OpenLib();
		if (lib == IntPtr.Zero) {
			throw new Exception("Unable to find libenchant. Tried: '" + LIBENCHANT_2_SO
				+ "', '" + LIBENCHANT_1_SO + "' and '" + LIBENCHANT_SO + "'.");
		}
		
		broker = BrokerInit(lib);
		if (broker == IntPtr.Zero) {
			throw new Exception("Unable to open enchant broker");
		}
	}
	
	public string[] GetProviders () {
		return BrokerDescribe(lib, broker);
	}
	
	public string[] GetLanguages () {
		return BrokerListDicts(lib, broker);
	}
	
	public int Close () {
		BrokerFree(lib, broker);

		return Interop.Interop.Close(lib);
	}


	/* Private methods */

	private IntPtr OpenLib () {
		//Try version 2
		IntPtr lib = Interop.Interop.Open(LIBENCHANT_2_SO);
		if (lib != IntPtr.Zero) {
			version = "2";
			return lib;
		}
		
		//Try version 1
		lib = Interop.Interop.Open(LIBENCHANT_1_SO);
		if (lib != IntPtr.Zero) {
			version = "1";
			return lib;
		}
		
		//Try fallback
		lib = Interop.Interop.Open(LIBENCHANT_SO);
		if (lib != IntPtr.Zero) {
			version = "unknown";
			return lib;
		}
		
		return IntPtr.Zero;
	}
	
	private IntPtr BrokerInit (IntPtr lib) {
		IntPtr method = Interop.Interop.GetMethod(lib, "enchant_broker_init");
		EnchantBrokerInitDelegate enchantBrokerInit = Marshal.GetDelegateForFunctionPointer(method, typeof(EnchantBrokerInitDelegate)) as EnchantBrokerInitDelegate;
		return enchantBrokerInit();
	}
	
	private void BrokerFree (IntPtr lib, IntPtr broker) {
		IntPtr method = Interop.Interop.GetMethod(lib, "enchant_broker_free");
		EnchantBrokerFreeDelegate enchantBrokerFree = Marshal.GetDelegateForFunctionPointer(method, typeof(EnchantBrokerFreeDelegate)) as EnchantBrokerFreeDelegate;
		enchantBrokerFree(broker);
	}

	private string[] BrokerDescribe (IntPtr lib, IntPtr broker) {
		IntPtr method = Interop.Interop.GetMethod(lib, "enchant_broker_describe");
		EnchantBrokerDescribeDelegate enchantBrokerDescribe = Marshal.GetDelegateForFunctionPointer(method, typeof(EnchantBrokerDescribeDelegate)) as EnchantBrokerDescribeDelegate;
		
		ArrayList providers = new ArrayList();
		enchantBrokerDescribe(broker, (providerName, providerDesc, providerFile, userdata) => providers.Add(providerName), IntPtr.Zero);
		
		return (string[])providers.ToArray(typeof(string));
	}
	
	private string[] BrokerListDicts (IntPtr lib, IntPtr broker) {
		IntPtr method = Interop.Interop.GetMethod(lib, "enchant_broker_list_dicts");
		EnchantBrokerListDictsDelegate enchantBrokerListDicts = Marshal.GetDelegateForFunctionPointer(method, typeof(EnchantBrokerListDictsDelegate)) as EnchantBrokerListDictsDelegate;
		
		ArrayList languages = new ArrayList();
		enchantBrokerListDicts (broker,	(langTag, providerName, providerDesc, providerFile, userdata) => languages.Add(langTag), IntPtr.Zero);

		return (string[])languages.ToArray(typeof(string));
	}

}

}
