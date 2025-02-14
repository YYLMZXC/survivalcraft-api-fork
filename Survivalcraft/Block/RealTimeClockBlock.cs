namespace Game
{
	public class RealTimeClockBlock : RotateableMountedElectricElementBlock
	{
		public static int Index = 187;

		public RealTimeClockBlock()
			: base("Models/Gates", "RealTimeClock", 0.5f)
		{
		}

		public override ElectricElement CreateElectricElement(SubsystemElectricity subsystemElectricity, int value, int x, int y, int z)
		{
			return new RealTimeClockElectricElement(subsystemElectricity, new CellFace(x, y, z, GetFace(value)));
		}

		public override ElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
		{
			int data = Terrain.ExtractData(value);
			if (GetFace(value) == face)
			{
				ElectricConnectorDirection? connectorDirection = SubsystemElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
				if (connectorDirection == ElectricConnectorDirection.Top || connectorDirection == ElectricConnectorDirection.Right || connectorDirection == ElectricConnectorDirection.Left || connectorDirection == ElectricConnectorDirection.Bottom || connectorDirection == ElectricConnectorDirection.In)
				{
					return ElectricConnectorType.Output;
				}
			}
			return null;
		}
	}
}
