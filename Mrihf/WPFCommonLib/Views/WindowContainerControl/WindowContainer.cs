﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Linq;
using System;
using WPFCommonLib.Extensions;

namespace WPFCommonLib.Views.WindowContainerControl
{
    public class WindowContainer : ContentControl
    {

        public static readonly DependencyProperty ShowProgressIndicatorProperty =
            DependencyProperty.Register("ShowProgressIndicator", typeof(bool), typeof(WindowContainer), new PropertyMetadata(false));

        public static readonly DependencyProperty WaitingMessageProperty =
            DependencyProperty.Register("WaitingMessage", typeof(string), typeof(WindowContainer), new PropertyMetadata("正在处理……"));

        static WindowContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowContainer), new FrameworkPropertyMetadata(typeof(WindowContainer)));
        }

        /// <summary>
        /// 是否显示等待遮罩
        /// </summary>
        public bool ShowProgressIndicator
        {
            get { return (bool)GetValue(ShowProgressIndicatorProperty); }
            set { SetValue(ShowProgressIndicatorProperty, value); }
        }

        /// <summary>
        /// 设置等待遮罩的提示文本
        /// </summary>
        public string WaitingMessage
        {
            get { return (string)GetValue(WaitingMessageProperty); }
            set { SetValue(WaitingMessageProperty, value); }
        }

        private const string PresentationGroup = "PresentationStates";


        public const string DefaultTransitionState = "DefaultTransition";
        public const string UpTransitionState = "UpTransition";
        public const string DownTransitionState = "DownTransition";
        public const string LeftTransitionState = "LeftTransition";
        public const string LeftTransitionExitState = "LeftTransitionExit";

        private ContentPresenter CurrentContentPresentationSite { get; set; }

        public string Transition
        {
            get { return GetValue(TransitionProperty) as string; }
            set { SetValue(TransitionProperty, value); }
        }

        public static readonly DependencyProperty TransitionProperty =
           DependencyProperty.Register(
               "Transition",
               typeof(string),
               typeof(WindowContainer),
               new PropertyMetadata(DefaultTransitionState, OnTransitionPropertyChanged));
        

        private Storyboard _currentTransition;

        private Storyboard CurrentTransition
        {
            get { return _currentTransition; }
            set
            {
                if (_currentTransition != null)
                {
                    _currentTransition.Completed -= OnTransitionCompleted;
                }

                _currentTransition = value;
                if (_currentTransition != null)
                {
                    _currentTransition.Completed += OnTransitionCompleted;
                }
            }
        }
        /// <summary>
        /// Occurs when the current transition has completed.
        /// </summary>
        public event RoutedEventHandler TransitionCompleted;
        private void OnTransitionCompleted(object sender, EventArgs e)
        {
            RoutedEventHandler handler = TransitionCompleted;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }
        private static void OnTransitionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowContainer source = (WindowContainer)d;
            string newTransition = e.NewValue as string;

            Storyboard newStoryboard = source.GetStoryboard(newTransition);

            source.CurrentTransition = newStoryboard;

        }
        private Storyboard GetStoryboard(string newTransition)
        {
            VisualStateGroup presentationGroup = VisualTreeHelperEx.TryGetVisualStateGroup(this, PresentationGroup);
            Storyboard newStoryboard = null;
            if (presentationGroup != null)
            {
                newStoryboard = presentationGroup.States
                    .OfType<VisualState>()
                    .Where(state => state.Name == newTransition)
                    .Select(state => state.Storyboard)
                    .FirstOrDefault();
            }
            return newStoryboard;
        }
        private void StartTransition(object oldContent, object newContent)
        {
            if (CurrentContentPresentationSite != null && Content != null)
            {
                CurrentContentPresentationSite.Content = newContent;
                VisualStateManager.GoToState(this, DefaultTransitionState, false);
                VisualStateManager.GoToState(this, LeftTransitionState, true);
                Transition = LeftTransitionState;
            }
            else if (Content == null)
            {
                VisualStateManager.GoToState(this, DefaultTransitionState, false);
                VisualStateManager.GoToState(this, LeftTransitionExitState, true);
                Transition = LeftTransitionExitState;
            }
            
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();


            CurrentContentPresentationSite = GetTemplateChild("contentPresenter") as ContentPresenter;
            if (CurrentContentPresentationSite != null)
            {
                CurrentContentPresentationSite.Content = Content;
            }


            //Storyboard transition = GetStoryboard(Transition);
            //CurrentTransition = transition;

            VisualStateManager.GoToState(this, DefaultTransitionState, false);

        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            StartTransition(oldContent, newContent);
        }
    }
}