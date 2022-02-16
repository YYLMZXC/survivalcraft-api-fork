using Engine;

namespace Game
{
	public class AnalogToDigitalConverterElectricElement : RotateableElectricElement
	{
		private int m_bits;

		public AnalogToDigitalConverterElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace)
			: base(subsystemElectricity, cellFace)
		{
		}

		public override float GetOutputVoltage(int face)
		{
			ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(base.CellFaces[0].Face, base.Rotation, face);
			if (connectorDirection.HasValue)
			{
				if (connectorDirection.Value == ElectricConnectorDirection.Top)
				{
					return (((uint)m_bits & (true ? 1u : 0u)) != 0) ? 1 : 0;
				}
				if (connectorDirection.Value == ElectricConnectorDirection.Right)
				{
					return (((uint)m_bits & 2u) != 0) ? 1 : 0;
				}
				if (connectorDirection.Value == ElectricConnectorDirection.Bottom)
				{
					return (((uint)m_bits & 4u) != 0) ? 1 : 0;
				}
				if (connectorDirection.Value == ElectricConnectorDirection.Left)
				{
					return (((uint)m_bits & 8u) != 0) ? 1 : 0;
				}
			}
			return 0f;
		}

		public override bool Simulate()
		{
			int bits = m_bits;
			int rotation = base.Rotation;
			foreach (ElectricConnection connection in base.Connections)
			{
				if (connection.ConnectorType != ElectricConnectorType.Output && connection.NeighborConnectorType != 0)
				{
					ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(base.CellFaces[0].Face, rotation, connection.ConnectorFace);
					if (connectorDirection.HasValue && connectorDirection.Value == ElectricConnectorDirection.In)
					{
						float outputVoltage = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
						m_bits = (int)MathUtils.Round(outputVoltage * 15f);
					}
				}
			}
			return m_bits != bits;
		}
	}
}
