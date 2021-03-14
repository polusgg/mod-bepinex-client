﻿namespace PolusApi.Resources {
	public struct CacheFile {
		public ResourceType Type;
		public byte[] Hash;
		public string Location;
		public byte[] Data;
		public string LocalLocation;
		public object ExtraData;
	}
}