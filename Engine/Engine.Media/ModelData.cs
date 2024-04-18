namespace Engine.Media
{
	public class ModelData
	{
		public List<ModelBoneData> Bones = [];

		public List<ModelMeshData> Meshes = [];

		public List<ModelBuffersData> Buffers = [];

		public static ModelFileFormat DetermineFileFormat(Stream stream)
		{
			if (Collada.IsColladaStream(stream))
			{
				return ModelFileFormat.Collada;
			}
			throw new InvalidOperationException("Unsupported model file format.");
		}

		public static ModelFileFormat DetermineFileFormat(string extension)
		{
			if (extension.Equals(".dae", StringComparison.OrdinalIgnoreCase))
			{
				return ModelFileFormat.Collada;
			}
			throw new InvalidOperationException("Unsupported model file format.");
		}

		public static ModelData Load(Stream stream, ModelFileFormat format)
		{
			if (format == ModelFileFormat.Collada)
			{
				return Collada.Load(stream);
			}
			throw new InvalidOperationException("Unsupported model file format.");
		}

		public static ModelData Load(string fileName, ModelFileFormat format)
		{
			using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
			{
				return Load(stream, format);
			}
		}

		public static ModelData Load(Stream stream)
		{
			var peekStream = new PeekStream(stream, 256);
			ModelFileFormat format = DetermineFileFormat(peekStream.GetInitialBytesStream());
			return Load(peekStream, format);
		}

		public static ModelData Load(string fileName)
		{
			using (Stream stream = Storage.OpenFile(fileName, OpenFileMode.Read))
			{
				return Load(stream);
			}
		}
	}
}
