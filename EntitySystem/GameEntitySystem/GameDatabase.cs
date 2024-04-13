using TemplatesDatabase;

namespace GameEntitySystem
{
	public class GameDatabase
	{
		public Database Database
		{
			get;
			set;
		}

		public DatabaseObjectType FolderType
		{
			get;
			set;
		}

		public DatabaseObjectType ProjectTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType MemberSubsystemTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType SubsystemTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType EntityTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType MemberComponentTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType ComponentTemplateType
		{
			get;
			set;
		}

		public DatabaseObjectType ParameterSetType
		{
			get;
			set;
		}

		public DatabaseObjectType ParameterType
		{
			get;
			set;
		}

		public GameDatabase(Database database)
		{
			Database = database;
			FolderType = database.FindDatabaseObjectType("Folder", throwIfNotFound: true);
			ProjectTemplateType = database.FindDatabaseObjectType("ProjectTemplate", throwIfNotFound: true);
			MemberSubsystemTemplateType = database.FindDatabaseObjectType("MemberSubsystemTemplate", throwIfNotFound: true);
			SubsystemTemplateType = database.FindDatabaseObjectType("SubsystemTemplate", throwIfNotFound: true);
			EntityTemplateType = database.FindDatabaseObjectType("EntityTemplate", throwIfNotFound: true);
			MemberComponentTemplateType = database.FindDatabaseObjectType("MemberComponentTemplate", throwIfNotFound: true);
			ComponentTemplateType = database.FindDatabaseObjectType("ComponentTemplate", throwIfNotFound: true);
			ParameterSetType = database.FindDatabaseObjectType("ParameterSet", throwIfNotFound: true);
			ParameterType = database.FindDatabaseObjectType("Parameter", throwIfNotFound: true);
		}
	}
}
