﻿using Acornima.Ast;
using Engine;
using System.Xml.Linq;

namespace Game
{
	public class SettingsControlsScreen : Screen
	{
		public ButtonWidget m_moveControlModeButton;

		public ButtonWidget m_lookControlModeButton;

		public ButtonWidget m_leftHandedLayoutButton;

		public ButtonWidget m_flipVerticalAxisButton;

		public ButtonWidget m_autoJumpButton;

		public ButtonWidget m_AllowInitialIntro;

		public ButtonWidget m_MemoryBankStyle;

		public ButtonWidget m_horizontalCreativeFlightButton;

		public ButtonWidget m_creativeDragMaxStackingButton;

		public ButtonWidget m_splitDragHalfButton;

		public ContainerWidget m_horizontalCreativeFlightPanel;

		public SliderWidget m_moveSensitivitySlider;

		public SliderWidget m_lookSensitivitySlider;

		public SliderWidget m_gamepadCursorSpeedSlider;

		public SliderWidget m_gamepadDeadZoneSlider;

		public SliderWidget m_creativeDigTimeSlider;

		public SliderWidget m_creativeReachSlider;

		public SliderWidget m_holdDurationSlider;

		public SliderWidget m_dragDistanceSlider;
		

		public SettingsControlsScreen()
		{
			XElement node = ContentManager.Get<XElement>("Screens/SettingsControlsScreen");
			LoadContents(this, node);
			m_moveControlModeButton = Children.Find<ButtonWidget>("MoveControlMode");
			m_lookControlModeButton = Children.Find<ButtonWidget>("LookControlMode");
			m_leftHandedLayoutButton = Children.Find<ButtonWidget>("LeftHandedLayout");
			m_flipVerticalAxisButton = Children.Find<ButtonWidget>("FlipVerticalAxis");
			m_AllowInitialIntro = Children.Find<ButtonWidget>("AllowInitialIntro");
			m_autoJumpButton = Children.Find<ButtonWidget>("AutoJump");
			m_horizontalCreativeFlightButton = Children.Find<ButtonWidget>("HorizontalCreativeFlight");
			m_horizontalCreativeFlightPanel = Children.Find<ContainerWidget>("HorizontalCreativeFlightPanel");
			m_creativeDragMaxStackingButton = Children.Find<ButtonWidget>("CreativeDragMaxStacking");
			m_splitDragHalfButton = Children.Find<ButtonWidget>("SplitDragHalf");
			m_moveSensitivitySlider = Children.Find<SliderWidget>("MoveSensitivitySlider");
			m_lookSensitivitySlider = Children.Find<SliderWidget>("LookSensitivitySlider");
			m_gamepadCursorSpeedSlider = Children.Find<SliderWidget>("GamepadCursorSpeedSlider");
			m_gamepadDeadZoneSlider = Children.Find<SliderWidget>("GamepadDeadZoneSlider");
			m_creativeDigTimeSlider = Children.Find<SliderWidget>("CreativeDigTimeSlider");
			m_creativeReachSlider = Children.Find<SliderWidget>("CreativeReachSlider");
			m_holdDurationSlider = Children.Find<SliderWidget>("HoldDurationSlider");
			m_dragDistanceSlider = Children.Find<SliderWidget>("DragDistanceSlider");
			m_MemoryBankStyle = Children.Find<ButtonWidget>("MemoryBankStyle");
			m_horizontalCreativeFlightPanel.IsVisible = true;
		}

		public override void Update()
		{
			if (m_moveControlModeButton.IsClicked)
			{
				SettingsManager.MoveControlMode = (MoveControlMode)((int)(SettingsManager.MoveControlMode + 1) % EnumUtils.GetEnumValues(typeof(MoveControlMode)).Count);
			}
			if (m_lookControlModeButton.IsClicked)
			{
				SettingsManager.LookControlMode = (LookControlMode)((int)(SettingsManager.LookControlMode + 1) % EnumUtils.GetEnumValues(typeof(LookControlMode)).Count);
			}
			if (m_leftHandedLayoutButton.IsClicked)
			{
				SettingsManager.LeftHandedLayout = !SettingsManager.LeftHandedLayout;
			}
			if (m_flipVerticalAxisButton.IsClicked)
			{
				SettingsManager.FlipVerticalAxis = !SettingsManager.FlipVerticalAxis;
			}
			if (m_autoJumpButton.IsClicked)
			{
				SettingsManager.AutoJump = !SettingsManager.AutoJump;
			}
			if (m_horizontalCreativeFlightButton.IsClicked)
			{
				SettingsManager.HorizontalCreativeFlight = !SettingsManager.HorizontalCreativeFlight;
			}
            if (m_creativeDragMaxStackingButton.IsClicked)
            {
                SettingsManager.CreativeDragMaxStacking = !SettingsManager.CreativeDragMaxStacking;
            }
			if(m_splitDragHalfButton.IsClicked)
			{
				SettingsManager.DragHalfInSplit = !SettingsManager.DragHalfInSplit;
			}
            if (m_moveSensitivitySlider.IsSliding)
			{
				SettingsManager.MoveSensitivity = m_moveSensitivitySlider.Value;
			}
			if (m_lookSensitivitySlider.IsSliding)
			{
				SettingsManager.LookSensitivity = m_lookSensitivitySlider.Value;
			}
			if (m_gamepadCursorSpeedSlider.IsSliding)
			{
				SettingsManager.GamepadCursorSpeed = m_gamepadCursorSpeedSlider.Value;
			}
			if (m_gamepadDeadZoneSlider.IsSliding)
			{
				SettingsManager.GamepadDeadZone = m_gamepadDeadZoneSlider.Value;
			}
			if (m_creativeDigTimeSlider.IsSliding)
			{
				SettingsManager.CreativeDigTime = m_creativeDigTimeSlider.Value;
			}
			if (m_creativeReachSlider.IsSliding)
			{
				SettingsManager.CreativeReach = m_creativeReachSlider.Value;
			}
			if (m_holdDurationSlider.IsSliding)
			{
				SettingsManager.MinimumHoldDuration = m_holdDurationSlider.Value;
			}
			if (m_dragDistanceSlider.IsSliding)
			{
				SettingsManager.MinimumDragDistance = m_dragDistanceSlider.Value;
			}
			if (m_MemoryBankStyle.IsClicked)
			{
				SettingsManager.UsePrimaryMemoryBank = !SettingsManager.UsePrimaryMemoryBank;
			}
			if (m_AllowInitialIntro.IsClicked) SettingsManager.AllowInitialIntro = !SettingsManager.AllowInitialIntro;
			m_moveControlModeButton.Text = LanguageControl.Get("MoveControlMode", SettingsManager.MoveControlMode.ToString());
			m_lookControlModeButton.Text = LanguageControl.Get("LookControlMode", SettingsManager.LookControlMode.ToString());
			m_leftHandedLayoutButton.Text = SettingsManager.LeftHandedLayout ? LanguageControl.On : LanguageControl.Off;
			m_flipVerticalAxisButton.Text = SettingsManager.FlipVerticalAxis ? LanguageControl.On : LanguageControl.Off;
			m_MemoryBankStyle.Text = SettingsManager.UsePrimaryMemoryBank ? LanguageControl.Get(GetType().Name, 2) : LanguageControl.Get(GetType().Name, 3);
			m_AllowInitialIntro.Text = SettingsManager.AllowInitialIntro ? LanguageControl.On : LanguageControl.Off;
			m_autoJumpButton.Text = SettingsManager.AutoJump ? LanguageControl.On : LanguageControl.Off;
			m_horizontalCreativeFlightButton.Text = SettingsManager.HorizontalCreativeFlight ? LanguageControl.On : LanguageControl.Off;
			m_creativeDragMaxStackingButton.Text = SettingsManager.CreativeDragMaxStacking ? LanguageControl.On : LanguageControl.Off;
			m_splitDragHalfButton.Text = SettingsManager.DragHalfInSplit ? LanguageControl.On : LanguageControl.Off;
			m_moveSensitivitySlider.Value = SettingsManager.MoveSensitivity;
			m_moveSensitivitySlider.Text = (SettingsManager.MoveSensitivity * 10f).ToString("0.0");
			m_lookSensitivitySlider.Value = SettingsManager.LookSensitivity;
			m_lookSensitivitySlider.Text = (SettingsManager.LookSensitivity * 10f).ToString("0.0");
			m_gamepadCursorSpeedSlider.Value = SettingsManager.GamepadCursorSpeed;
			m_gamepadCursorSpeedSlider.Text = $"{SettingsManager.GamepadCursorSpeed:0.0}x";
			m_gamepadDeadZoneSlider.Value = SettingsManager.GamepadDeadZone;
			m_gamepadDeadZoneSlider.Text = $"{SettingsManager.GamepadDeadZone * 100f:0}%";
			m_creativeDigTimeSlider.Value = SettingsManager.CreativeDigTime;
			m_creativeDigTimeSlider.Text = $"{MathF.Round(1000f * SettingsManager.CreativeDigTime)}ms";
			m_creativeReachSlider.Value = SettingsManager.CreativeReach;
			m_creativeReachSlider.Text = string.Format(LanguageControl.Get(GetType().Name, 0), $"{SettingsManager.CreativeReach:0.0} ");
			m_holdDurationSlider.Value = SettingsManager.MinimumHoldDuration;
			m_holdDurationSlider.Text = $"{MathF.Round(1000f * SettingsManager.MinimumHoldDuration)}ms";
			m_dragDistanceSlider.Value = SettingsManager.MinimumDragDistance;
			m_dragDistanceSlider.Text = $"{MathF.Round(SettingsManager.MinimumDragDistance)} " + LanguageControl.Get(GetType().Name, 1);
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
			}
		}
	}
}
