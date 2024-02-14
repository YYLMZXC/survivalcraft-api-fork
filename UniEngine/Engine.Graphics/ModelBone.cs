using System.Collections.Generic;

namespace Engine.Graphics
{
	public class ModelBone
	{
		internal List<ModelBone> m_childBones = [];

		internal Matrix m_transform;

		public Model Model
		{
			get;
			internal set;
		}

		public int Index
		{
			get;
			internal set;
		}

		public string Name
		{
			get;
			set;
		}

		public Matrix Transform
		{
			get
			{
				return m_transform;
			}
			set
			{
				m_transform = value;
			}
		}

		public ModelBone ParentBone
		{
			get;
			internal set;
		}

		public ReadOnlyList<ModelBone> ChildBones => new(m_childBones);

		internal ModelBone()
		{
		}
	}
}
