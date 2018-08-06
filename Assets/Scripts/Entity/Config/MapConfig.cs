namespace ETModel
{
	[Config(AppType.Client)]
	public partial class MapConfigCategory : ACategory<MapConfig>
	{
	}

	public class MapConfig: IConfig
	{
		public long Id { get; set; }
		public string Desc;
		public int ResourcesID;
	}
}
