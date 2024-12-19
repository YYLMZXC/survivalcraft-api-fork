using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game;

public class SubsystemSeasons : Subsystem, IUpdateable
{
	public SubsystemGameInfo m_subsystemGameInfo;

	private static Image m_seasonsGradient;

	public static readonly float SummerStart = 0f;

	public static readonly float AutumnStart = 0.25f;

	public static readonly float WinterStart = 0.5f;

	public static readonly float SpringStart = 0.75f;

	public static readonly float MidSummer = IntervalUtils.Midpoint(SummerStart, AutumnStart);

	public static readonly float MidAutumn = IntervalUtils.Midpoint(AutumnStart, WinterStart);

	public static readonly float MidWinter = IntervalUtils.Midpoint(WinterStart, SpringStart);

	public static readonly float MidSpring = IntervalUtils.Midpoint(SpringStart, SummerStart);

	public Season Season { get; private set; }

	public float TimeOfSeason { get; private set; }

	public UpdateOrder UpdateOrder => UpdateOrder.Default;

	public const string fName = "SubsystemSeasons";

	public static string GetTimeOfYearName(float timeOfYear)
	{
		TimeOfYearToSeason(timeOfYear, out var season, out var timeOfSeason);
		int num = timeOfSeason switch
		{
			< 0.25f => 0,
			>= 0.75f => 2,
			_ => 1
		};
		return LanguageControl.Get(fName, (int)season * 3 + num);
	}

	public static Color GetTimeOfYearColor(float timeOfYear)
	{
		if (m_seasonsGradient == null)
		{
			m_seasonsGradient = (Image)ContentManager.Get<Texture2D>("Textures/Gui/SeasonsSlider").Tag;
		}
		int x = (int)Math.Clamp(MathF.Round(timeOfYear * (float)m_seasonsGradient.Width), 0f, m_seasonsGradient.Width - 1);
		return m_seasonsGradient.GetPixel(x, 0);
	}

	public override void Load(ValuesDictionary valuesDictionary)
	{
		m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(throwOnError: true);
	}

	public void Update(float dt)
	{
		TimeOfYearToSeason(m_subsystemGameInfo.WorldSettings.TimeOfYear, out var season, out var timeOfSeason);
		Season = season;
		TimeOfSeason = timeOfSeason;
	}

	private static void TimeOfYearToSeason(float timeOfYear, out Season season, out float timeOfSeason)
	{
		if (IntervalUtils.IsBetween(timeOfYear, SummerStart, AutumnStart))
		{
			season = Season.Summer;
			timeOfSeason = IntervalUtils.Interval(SummerStart, timeOfYear) / IntervalUtils.Interval(SummerStart, AutumnStart);
		}
		else if (IntervalUtils.IsBetween(timeOfYear, AutumnStart, WinterStart))
		{
			season = Season.Autumn;
			timeOfSeason = IntervalUtils.Interval(AutumnStart, timeOfYear) / IntervalUtils.Interval(AutumnStart, WinterStart);
		}
		else if (IntervalUtils.IsBetween(timeOfYear, WinterStart, SpringStart))
		{
			season = Season.Winter;
			timeOfSeason = IntervalUtils.Interval(WinterStart, timeOfYear) / IntervalUtils.Interval(WinterStart, SpringStart);
		}
		else
		{
			season = Season.Spring;
			timeOfSeason = IntervalUtils.Interval(SpringStart, timeOfYear) / IntervalUtils.Interval(SpringStart, SummerStart);
		}
	}
}
