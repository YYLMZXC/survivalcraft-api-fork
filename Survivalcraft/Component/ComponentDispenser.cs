using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentDispenser : ComponentInventoryBase
	{
		public SubsystemTerrain m_subsystemTerrain;

		public SubsystemAudio m_subsystemAudio;

		public SubsystemPickables m_subsystemPickables;

		public SubsystemProjectiles m_subsystemProjectiles;

		public ComponentBlockEntity m_componentBlockEntity;

		public virtual void Dispense()
		{
			Point3 coordinates = m_componentBlockEntity.Coordinates;
			int data = Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
			int direction = DispenserBlock.GetDirection(data);
			DispenserBlock.Mode mode = DispenserBlock.GetMode(data);
			int num = 0;
			int slotValue;
			while (true)
			{
				if (num < SlotsCount)
				{
					slotValue = GetSlotValue(num);
					int slotCount = GetSlotCount(num);
					if (slotValue != 0 && slotCount > 0 && BlocksManager.Blocks[Terrain.ExtractContents(slotValue)].CanBeFiredByDispenser(slotValue))
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			ModsManager.HookAction("DispenserChooseItemToDispense", loader =>
			{
				loader.DispenserChooseItemToDispense(this, ref num, ref slotValue, out bool chosen);
				return chosen;
			});
			if(num >= 0)
			{
                int num2 = RemoveSlotItems(num, 1);
                for (int i = 0; i < num2; i++)
                {
                    DispenseItem(coordinates, direction, slotValue, mode);
                }
            }
		}

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
			m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(throwOnError: true);
			m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(throwOnError: true);
			m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(throwOnError: true);
			m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(throwOnError: true);
		}

		public virtual void DispenseItem(Point3 point, int face, int value, DispenserBlock.Mode mode)
		{
			Vector3 vector = CellFace.FaceToVector3(face);
			Vector3 position = new Vector3(point.X + 0.5f, point.Y + 0.5f, point.Z + 0.5f) + (0.6f * vector);
			//Ͷ����Ʒ
			if (mode == DispenserBlock.Mode.Dispense)
			{
				float s = 1.8f;
				Pickable pickable = m_subsystemPickables.CreatePickable(value, 1, position, s * (vector + m_random.Vector3(0.2f)), null);
                ModsManager.HookAction("OnDispenserDispensePickable", loader =>
				{
					loader.OnDispenserDispense(this, ref pickable);
					return false;
                });
				if (pickable != null) {
					m_subsystemPickables.AddPickable(pickable);
					m_subsystemAudio.PlaySound("Audio/DispenserDispense", 1f, 0f, new Vector3(position.X, position.Y, position.Z), 3f, autoDelay: true);
					return;
				}
				return;
			}
			//������Ʒ
			float s2 = m_random.Float(39f, 41f);
            if (m_subsystemProjectiles.CanFireProjectile(value, position, vector, null, out Vector3 position2))
            {
                Projectile projectile = m_subsystemProjectiles.CreateProjectile(value, position2, s2 * (vector + m_random.Vector3(0.025f) + new Vector3(0f, 0.05f, 0f)), Vector3.Zero, null);
                projectile.Creator = this;
				projectile.OwnerEntity = Entity;
                ModsManager.HookAction("OnDispenserShoot", loader =>
                {
					loader.OnDispenserShoot(this, ref projectile);
                    return false;
                });
				if (projectile != null)
                {
                    m_subsystemProjectiles.FireProjectileFast<Projectile>(projectile);
                    m_subsystemAudio.PlaySound("Audio/DispenserShoot", 1f, 0f, new Vector3(position.X, position.Y, position.Z), 4f, autoDelay: true);
					return;
                }
				else
				{
					return;
				}
			}
			else
			{
				DispenseItem(point, face, value, DispenserBlock.Mode.Dispense);
			}
		}
	}
}
