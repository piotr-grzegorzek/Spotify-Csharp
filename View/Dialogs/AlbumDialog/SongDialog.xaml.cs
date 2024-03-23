using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using NAudio.Wave;
using Studio.Logic;

namespace Studio {
    /// <summary>
    /// Interaction logic for StudioNameDialog.xaml
    /// </summary>
    public partial class SongDialog : MetroWindow {
        private Mp3FileReader reader;
        private WaveOutEvent player = new WaveOutEvent();
        private DispatcherTimer play_timer = new DispatcherTimer();
        private bool playing = false;
        public SongDialog(Song song) {
            InitializeComponent();
            Title = song.Name;

            songImage.ImageSource = song.Image;
            songName.Text = song.Name;
            songAlbum.Inlines.Add((string) Application.Current.Resources["From"] + " ");
            songAlbum.Inlines.Add(new Run(song.Album) { FontStyle = FontStyles.Italic });

            Database db = DatabaseFactory.init();
            Byte[] song_bin = db.getSong(song.Name);
            MemoryStream ms = new MemoryStream(song_bin);
            reader = new Mp3FileReader(ms);
            player.Init(reader);

            play_timer.Interval = System.TimeSpan.FromSeconds(1);
            play_timer.Tick += timer_tick;
            progress.Maximum = reader.TotalTime.TotalSeconds;
            total_time.Text = readable(reader.TotalTime.TotalSeconds);
            shortcuts();
        }
        private void shortcuts() {
            //play - space
            RoutedCommand play = new RoutedCommand();
            play.InputGestures.Add(new KeyGesture(Key.Space));
            CommandBindings.Add(new CommandBinding(play, SongButon_Click));
        }
        private void SongButon_Click(object sender, RoutedEventArgs e) {
            if (!playing) {
                player.Play();
                play_timer.Start();
                playing = true;
                SongButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Pause;
            }
            else {
                player.Pause();
                play_timer.Stop();
                playing = false;
                SongButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            }
        }
        private void timer_tick(object sender, System.EventArgs e) {
            if (reader.CurrentTime.TotalSeconds >= progress.Maximum) {
                player.Stop();
                WaveStreamExtensions.SetPosition(reader, 0);
                play_timer.Stop();
                playing = false;
                SongButtonIcon.Kind = MahApps.Metro.IconPacks.PackIconMaterialKind.Play;
            }
            progress.Value = reader.CurrentTime.TotalSeconds;
        }
        private void progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            current_time.Text = readable(progress.Value);
        }
        private void progress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            play_timer.Stop();
        }
        private void progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            WaveStreamExtensions.SetPosition(reader, progress.Value);
            if (playing) {
                player.Play();
                play_timer.Start();
            }
        }
        private void progress_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            //Track click
            WaveStreamExtensions.SetPosition(reader, progress.Value);
        }
        private string readable(double time) {
            int seconds = (int) time;
            string minutes = $"{seconds / 60}";
            string sub_seconds = $"{seconds % 60}";
            if (minutes.Length == 1)
                minutes = "0" + minutes;
            if (sub_seconds.Length == 1)
                sub_seconds = "0" + sub_seconds;
            return $"{minutes}:{sub_seconds}";
        }
        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            player.Stop();
        }
        private void okButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
        private void cancelButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}