using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NAudio.Wave;
using NAudio.Gui;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Timers;
using System.Windows.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using NAudio.Midi;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Messaging;
using System.Linq.Expressions;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using System.Windows.Media;
using System.Windows.Shapes;
using static System.Windows.Forms.LinkLabel;

namespace promaker
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //private string[,] wavFiles;
        private List<List<string>> wavFiles;// = new List<List<string>>();
        private System.Windows.Controls.TextBox[] fileTextBoxes;
        //private WaveOutEvent[,] waveOuts = new WaveOutEvent[0,0];
        private List<List<WaveOutWrapper>> WaveOuts = new List<List<WaveOutWrapper>>();

        Queue<AudioPlaybackInfo> queue = new Queue<AudioPlaybackInfo>();

        private int folderIndex;
        bool playbool = false;
        private float volume;

        // Register global hotkeys
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // Unregister global hotkeys
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const uint HOTKEY_BASE_ID = 9000;
        private const uint MOD_NONE = 0x0000;
        private const uint MOD_SHIFT = 0x0004;

        //
        //public ObservableCollection<IntervalData> Intervals { get; set; }
        // 폴더 정보와 인덱스를 저장하는 클래스
        public class FolderInfo
        {
            public string FolderPath { get; set; }
            public int Index { get; set; }

            public FolderInfo(string folderPath, int index)
            {
                FolderPath = folderPath;
                Index = index;
            }
        }

        public class WaveOutWrapper
        {
            public WaveOutEvent WaveOut { get; set; }
            public bool IsInitialized { get; private set; }
            public MemoryStream MS { get; set; }
            public WaveOutWrapper()
            {
                WaveOut = new WaveOutEvent();
                IsInitialized = false;
            }
            public void Init(WaveStream stream)
            {
                WaveOut.Init(stream); IsInitialized = true;
            }
        }

        public struct AudioPlaybackInfo
        {
            public float Interval { get; set; }
            public WaveOutEvent WaveOut { get; set; }
            public MemoryStream MS { get; set; }
            public float PlayTime { get; set; }
            public float Volumes { get; set; }
        }

        // 리스트 선언 및 초기화
        private List<AudioPlaybackInfo> playbackList = new List<AudioPlaybackInfo>();


        public MainWindow()
        {
            //RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            InitializeComponent();
        }

        // MouseEnter 이벤트 핸들러 함수
        private void NumEventHandler(object sender, EventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                // 예시로 특정 텍스트를 설정하는 코드
                textBox.Text = midindextxt.Text;
            }
        }

        private void Num_1_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile1.Text);
        }

        private void Num_2_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile2.Text);
        }

        private void Num_3_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile3.Text);
        }

        private void Num_4_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile4.Text);
        }

        private void Num_5_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile5.Text);
        }

        private void Num_6_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile6.Text);
        }

        private void Num_7_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile7.Text);
        }

        private void Num_8_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile8.Text);
        }

        private void Num_9_Click(object sender, RoutedEventArgs e)
        {
            PlayNumluancher(txtFile9.Text);
        }

        private void PlayNumluancher(string txtfile)
        {
            string textValue = txtfile; // 예: "2,5"
            string[] indices = textValue.Split(',');         // 콤마로 구분

            if (indices.Length == 2)
            {
                int folderIndex = int.Parse(indices[0]);     // 첫 번째 값: 폴더 인덱스
                int fileIndex = int.Parse(indices[1]);       // 두 번째 값: 파일 인덱스

                // 이제 folderIndex와 fileIndex를 사용할 수 있습니다
                Console.WriteLine($"Folder Index: {folderIndex}, File Index: {fileIndex}");

                Task.Run(() => PlayWav(folderIndex, fileIndex, float.Parse(playtimetxt.Text)));

                RecordProcess(txtfile);
            }
            else
            {
                // 오류 처리 (예상치 못한 값의 경우)
                Console.WriteLine("텍스트 형식이 잘못되었습니다.");
            }
        }
        bool IsDirectory(string path)
        {
            // 경로의 속성을 가져옴
            FileAttributes attr = File.GetAttributes(path);

            // 디렉토리인지 확인
            return (attr & FileAttributes.Directory) == FileAttributes.Directory;
        }

        private void folderbtn_Click(object sender, RoutedEventArgs e)
        {
            folderload();
        }

        private void folderload()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                string currentDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
                string soundsetDirectory = System.IO.Path.Combine(currentDirectory, "soundset");

                if (Directory.Exists(soundsetDirectory))
                {
                    //int totalFolders = Directory.GetDirectories(soundsetDirectory).Length + 2;

                    wavFiles = new List<List<string>>(); // 빈 배열로 초기화 // 빈 배열로 초기화
                    wavFiles.Add(new List<string>()); // 각 폴더에 대해 새로운 리스트 추가
                    WaveOuts = new List<List<WaveOutWrapper>>(); // 빈 배열로 초기화 // 빈 배열로 초기화
                    WaveOuts.Add(new List<WaveOutWrapper>()); // 각 폴더에 대해 새로운 리스트 추가
                    folderIndex = 0;

                    TreeViewItem rootNode = CreateTreeItemForFolder(soundsetDirectory);
                    wavtree.Items.Add(rootNode);

                    // 첫 번째 부모 노드 선택
                    if (wavtree.Items.Count > 0 && wavtree.Items[0] is TreeViewItem firstParentNode && firstParentNode.Items.Count > 0)
                    {
                        // 세 번째 자식 노드 선택
                        if (firstParentNode.Items[0] is TreeViewItem thirdChildNode)
                        {
                            thirdChildNode.IsSelected = true;       // 세 번째 자식 선택
                            thirdChildNode.BringIntoView();         // 뷰로 가져오기

                            // 세 번째 자식 노드의 첫 번째 자식 선택
                            if (thirdChildNode.Items.Count > 0 && thirdChildNode.Items[7] is TreeViewItem grandChildNode)
                            {
                                grandChildNode.IsSelected = true;       // 첫 번째 자식의 자식 선택
                                grandChildNode.BringIntoView();         // 뷰로 가져오기
                            }

                            wavtree_selectedChangeFunction();       // 선택 후 동작 수행

                            //functionstart1();
                        }
                    }

                    return;
                }

                dialog.Description = "폴더를 선택하세요";  // 대화 상자의 설명
                dialog.SelectedPath = currentDirectory;    // 기본 경로를 현재 실행된 폴더로 설정

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //int totalFolders = Directory.GetDirectories(dialog.SelectedPath).Length+2;

                    wavFiles = new List<List<string>>(); // 빈 배열로 초기화 // 빈 배열로 초기화
                    wavFiles.Add(new List<string>()); // 각 폴더에 대해 새로운 리스트 추가
                    WaveOuts = new List<List<WaveOutWrapper>>(); // 빈 배열로 초기화 // 빈 배열로 초기화
                    WaveOuts.Add(new List<WaveOutWrapper>()); // 각 폴더에 대해 새로운 리스트 추가
                    folderIndex = 0;

                    TreeViewItem rootNode = CreateTreeItemForFolder(dialog.SelectedPath);
                    wavtree.Items.Add(rootNode);
                }
            }

        }

        private TreeViewItem CreateTreeItemForFolder(string folderPath)
        {

            // 폴더 이름으로 트리뷰 항목 생성
            TreeViewItem folderNode = new TreeViewItem
            {
                Header = System.IO.Path.GetFileName(folderPath), //.GetFileName(folderPath),
                Tag = new FolderInfo(folderPath, folderIndex)
            };

            // 해당 폴더 안의 WAV 파일 추가
            string[] wavFilessub = Directory.GetFiles(folderPath, "*.wav");
            if (wavFilessub.Length < 2)
            {
                wavFilessub = Directory.GetFiles(folderPath, "*.mp3");
            }
            int fileIndex = 0;

            foreach (string file in wavFilessub)
            {

                TreeViewItem fileNode = new TreeViewItem
                {
                    Header = System.IO.Path.GetFileName(file),
                    Tag = new FolderInfo(file, fileIndex)
                };

                folderNode.Items.Add(fileNode); // 파일 노드를 폴더 노드에 추가

                //using (var reader = wavFiles[folderIndex][fileIndex].EndsWith(".wav") ? (WaveStream)new WaveFileReader(memoryStream) : (WaveStream)new Mp3FileReader(memoryStream))
                using (var reader = file.EndsWith(".wav") ? (WaveStream)new WaveFileReader(file) : (WaveStream)new Mp3FileReader(file))
                {
                    WaveOutWrapper item = new WaveOutWrapper();
                    //item.Init(reader);
                    WaveOuts[folderIndex].Add(item);//, fileIndex].Init(reader);// = reader;  // 폴더의 파일을 배열에 추가
                }

                wavFiles[folderIndex].Add(file);
                fileIndex++;

            }

            // 하위 폴더가 있으면 재귀적으로 추가
            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders)
            {
                wavFiles.Add(new List<string>()); // 각 폴더에 대해 새로운 리스트 추가
                WaveOuts.Add(new List<WaveOutWrapper>());
                folderIndex++;
                folderNode.Items.Add(CreateTreeItemForFolder(subFolder));
            }

            return folderNode;

        }

        private async void PlayWav(int folderIndex2, int index, float playTimeMs)
        {
            //if (File.Exists(wavFiles[folderIndex2][index]))
            //{
            /*
            // 기존 사운드를 중지하고 새로 재생 (동시에 여러 사운드 가능)
            if (waveOuts[folderIndex2, index] != null)
            {
                waveOuts[folderIndex2, index].Stop();
                waveOuts[folderIndex2, index].Dispose();
            }
            */
            // 새로운 AudioFileReader와 WaveOutEvent 객체를 생성하여 재생

            if (folderIndex2 < wavFiles.Count && index < wavFiles[folderIndex2].Count)
            {
                /*
                var audioFileReader = new AudioFileReader(wavFiles[folderIndex2][index]);
                var waveOut = new WaveOutEvent(); // 중복 재생을 허용하기 위해 새로운 객체 생성
                waveOut.Init(audioFileReader);
                waveOut.Play();

                // 지정된 시간 만큼 대기
                await Task.Delay((int)playTimeMs);
                //Thread.Sleep((int)playTimeMs);

                // 사운드 중지 및 리소스 해제
                waveOut.Stop();
                waveOut.Dispose();
                audioFileReader.Dispose();
                */
                //var waveOut = PrepareAudioFromMemory(wavFiles[folderIndex2][index]);
                MemoryStream memoryStream = CopyToMemoryStream(wavFiles[folderIndex2][index]);
                //using (var reader = new WaveFileReader(wavFiles[folderIndex2][index]))
                //using (var reader = wavFiles[folderIndex2][index].EndsWith(".wav")?(WaveStream)new WaveFileReader(wavFiles[folderIndex2][index]):(WaveStream)new Mp3FileReader(wavFiles[folderIndex2][index]))
                using (var reader = wavFiles[folderIndex2][index].EndsWith(".wav") ? (WaveStream)new WaveFileReader(memoryStream) : (WaveStream)new Mp3FileReader(memoryStream))
                {

                    //WaveFormat waveFormat2 = reader.WaveFormat;
                    // WaveFormat 정보 출력
                    //var waveFormat = new WaveFormat(44100, 16, 2); // 필요한 형식으로 조정
                    //var waveStream = new RawSourceWaveStream(memoryStream, waveFormat2);
                    var waveOut = new WaveOutEvent();
                    waveOut.Init(reader);  //waveStream
                    //await Task.Delay((int)1000);
                    // 재생
                    waveOut.Volume = volume;
                    waveOut.Play();
                    await Task.Delay((int)playTimeMs);
                    waveOut.Stop();
                    waveOut.Dispose();

                }
                memoryStream.Dispose();

            }
            //}
        }
        MemoryStream CopyToMemoryStream(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);
            return new MemoryStream(fileBytes);
        }

        private WaveOutEvent PrepareAudio(int folderIndex2, int index, out MemoryStream memoryStream)
        {
            // Ensure the memory stream is initialized
            memoryStream = null;
            WaveOutEvent waveOut = null;

            if (folderIndex2 < wavFiles.Count && index < wavFiles[folderIndex2].Count)
            {
                memoryStream = CopyToMemoryStream(wavFiles[folderIndex2][index]);

                WaveStream reader = null;
                try
                {
                    // Choose the appropriate reader for the file format (wav or mp3)
                    if (wavFiles[folderIndex2][index].EndsWith(".wav"))
                    {
                        reader = new WaveFileReader(memoryStream);
                    }
                    else
                    {
                        reader = new Mp3FileReader(memoryStream);
                    }

                    reader.Position = 0;

                    // Initialize the wave output
                    waveOut = new WaveOutEvent();
                    waveOut.Volume = volume;// (internalvolume == 0) ? 1.0f : internalvolume;
                    waveOut.Init(reader);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing audio: {ex.Message}");
                    reader?.Dispose();
                }
            }

            return waveOut;
        }

        // 미리 준비한 오디오 파일을 재생하는 메서드
        private async Task PlayPreparedAudioAsync(WaveOutEvent waveOut, float playTimeMs, MemoryStream MS, float internalvolume)
        {
            if (internalvolume == 0.0f)
            {
                try
                {
                    if (waveOut != null)
                    {
                        waveOut.Stop();
                        waveOut.Dispose();
                        MS.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        scalesettingtxt.AppendText(ex.Message);
                    });
                    // Log or handle the exception here
                    //Console.WriteLine($"An error occurred: {ex.Message}");
                }

            }
            else
            {
                if (waveOut != null)
                {
                    Task.Run(() => playtask(waveOut, playTimeMs, MS, internalvolume));
                }
            }
        }

        private async Task playtask(WaveOutEvent waveOut, float playTimeMs, MemoryStream MS, float internalvolume)
        {
            try
            {
                waveOut.Volume = internalvolume;
                waveOut.Play();
                await Task.Delay((int)playTimeMs);

                waveOut.Stop();
                //waveOut.Dispose();
                //MS.Dispose();
            }
            catch (Exception ex)
            {
                scalesettingtxt.AppendText(ex.ToString());
            }
        }
        private async Task disposetask()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    PlayBTN.IsEnabled = false;
                    playbool = false;
                    LoadFilesBTN.IsEnabled = false;
                });
                int playlinecnt = 0;
                int playColumnCnt = 0;
                int playColumnMaxCnt = datagridview1.Columns.Count - 2;
                string totalline = datagridview1.Items.Count.ToString();

                foreach (var item in playbackList)
                {

                    await Dispatcher.InvokeAsync(() =>
                    {
                        currentplaylinetxt.Text = playlinecnt.ToString() + " / " + totalline;
                    });

                    await Dispatcher.InvokeAsync(() =>
                    {
                        int rowIndex = playlinecnt - 1;
                        if (rowIndex >= 0 && rowIndex < datagridview1.Items.Count)
                        {
                            datagridview1.SelectedCells.Clear();
                            for (int columnIndex = 0; columnIndex < datagridview1.Columns.Count; columnIndex++)
                            {
                                datagridview1.SelectedCells.Add(new DataGridCellInfo(datagridview1.Items[rowIndex], datagridview1.Columns[columnIndex]));
                            }

                            // Ensure the selected cell is scrolled into view
                            if (datagridview1.Items.Count - 1 > rowIndex + 5)
                            {
                                datagridview1.ScrollIntoView(datagridview1.Items[rowIndex + 5]);
                            }
                            else
                            {
                                datagridview1.ScrollIntoView(datagridview1.Items[datagridview1.Items.Count - 1]);
                            }

                        }
                    });


                    float interval2 = item.Interval;
                    WaveOutEvent waveOut = item.WaveOut;
                    MemoryStream MS = item.MS;
                    float playTime = item.PlayTime;
                    float internalvolume = item.Volumes;

                    if (waveOut != null)
                    {
                        waveOut.Stop();
                        waveOut.Dispose();
                        MS.Dispose();
                    }

                    if (interval2 != 0) { playlinecnt++; }

                }
                playbool = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    LoadFilesBTN.IsEnabled = true;
                });

            }
            catch (Exception ex)
            {
                scalesettingtxt.AppendText(ex.Message);
            }

            await Dispatcher.InvokeAsync(() =>
            {
                datagridview1.UnselectAll();
            });
        }



        private void registGlobalKey(bool regiorclose)
        {
            if (regiorclose)
            {
                var helper = new WindowInteropHelper(this);

                for (uint vk = 0x61; vk <= 0x69; vk++)
                {
                    RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + (vk - 0x61)), MOD_NONE, vk);
                }

                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 10), MOD_NONE, (uint)0x25); // Left key
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 11), MOD_NONE, (uint)0x27); // Right key
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 12), MOD_SHIFT, (uint)0x25); // Shift + Left key
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 13), MOD_SHIFT, (uint)0x27);

                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 14), MOD_SHIFT, (uint)0x70); //F1: 0x70
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 15), MOD_SHIFT, (uint)0x71); //F2: 0x71
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 16), MOD_SHIFT, (uint)0x72); //F3: 0x72
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 17), MOD_SHIFT, (uint)0x73); //F4: 0x73
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 18), MOD_SHIFT, (uint)0x74); //F5: 0x74
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 19), MOD_NONE, (uint)0x70); //F1: 0x70
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 20), MOD_NONE, (uint)0x71); //F2: 0x71
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 21), MOD_NONE, (uint)0x72); //F3: 0x72
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 22), MOD_NONE, (uint)0x73); //F4: 0x73
                RegisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + 23), MOD_NONE, (uint)0x74); //F5: 0x74

                ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
            }
            else
            {
                var helper = new WindowInteropHelper(this);
                for (uint vk = 0x61; vk <= 0x69; vk++)
                {
                    UnregisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + (vk - 0x61)));
                }

                for (int rk = 10; rk < 23; rk++)
                {
                    UnregisterHotKey(helper.Handle, (int)(HOTKEY_BASE_ID + rk));
                }
            }
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == 0x0312) // WM_HOTKEY
            {
                int keyIndex = (int)(msg.wParam.ToInt32() - HOTKEY_BASE_ID); // Key index based on HOTKEY_ID
                if (keyIndex >= 0 && keyIndex < 9)  // 0~9
                {
                    string textValue = fileTextBoxes[keyIndex].Text;
                    string[] indices = textValue.Split(',');

                    if (indices.Length == 2)
                    {
                        int folderIndex = int.Parse(indices[0]);
                        int fileIndex = int.Parse(indices[1]);
                        Console.WriteLine($"Folder Index: {folderIndex}, File Index: {fileIndex}");
                        Task.Run(() => PlayWav(folderIndex, fileIndex, float.Parse(playtimetxt.Text)));
                    }
                    else
                    {
                        Console.WriteLine("텍스트 형식이 잘못되었습니다.");
                    }
                }
                else if (keyIndex >= 10 && keyIndex <= 23) //10~13
                {
                    switch (keyIndex)
                    {
                        case 10:
                            // Handle Left key action
                            if (playtimeslider.Value > 31)
                            {
                                playtimeslider.Value = 32;
                            }
                            else
                            {
                                playtimeslider.Value = playtimeslider.Value + 1;
                            }
                            break;
                        case 11:
                            // Handle Right key action
                            if (playtimeslider.Value < 2)
                            {
                                playtimeslider.Value = 1;
                            }
                            else
                            {
                                playtimeslider.Value = playtimeslider.Value - 1;
                            }
                            break;
                        case 12:
                            // Handle Shift + Left key action
                            if (playintervalslider.Value < 20)
                            {
                                playintervalslider.Value = 10;
                            }
                            else
                            {
                                playintervalslider.Value = playintervalslider.Value - 10;
                            }
                            break;
                        case 13:
                            // Handle Shift + Right key action
                            if (playintervalslider.Value > 390)
                            {
                                playintervalslider.Value = 400;
                            }
                            else
                            {
                                playintervalslider.Value = playintervalslider.Value + 10;
                            }
                            break;
                        case 14: //Shift + F1:
                            playintervalslider.Value = float.Parse(txtf1.Text);
                            break;
                        case 15: //Shift + F2:
                            playintervalslider.Value = float.Parse(txtf2.Text);
                            break;
                        case 16: //Shift + F3:
                            playintervalslider.Value = float.Parse(txtf3.Text);
                            break;
                        case 17: //Shift + F4:
                            playintervalslider.Value = float.Parse(txtf4.Text);
                            break;
                        case 18: //Shift + F5:
                            playintervalslider.Value = float.Parse(txtf5.Text);
                            break;
                        case 19: //F1:
                            playtimeslider.Value = 2;
                            break;
                        case 20: //F2:
                            playtimeslider.Value = 4;
                            break;
                        case 21: //F3:
                            playtimeslider.Value = 8;
                            break;
                        case 22: //F4:
                            playtimeslider.Value = 16;
                            break;
                        case 23: //F5:
                            playtimeslider.Value = 32;
                            break;
                        default:
                            Console.WriteLine("Unrecognized hotkey.");
                            break;
                    }
                }

            }

        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key >= Key.NumPad1 && e.Key <= Key.NumPad9)
            {
                // 숫자패드의 1~9 키에 대응
                int keyIndex = e.Key - Key.NumPad1;

                string textValue = fileTextBoxes[keyIndex].Text; // 예: "2,5"
                string[] indices = textValue.Split(',');         // 콤마로 구분

                if (indices.Length == 2)
                {
                    int folderIndex = int.Parse(indices[0]);     // 첫 번째 값: 폴더 인덱스
                    int fileIndex = int.Parse(indices[1]);       // 두 번째 값: 파일 인덱스

                    // 이제 folderIndex와 fileIndex를 사용할 수 있습니다
                    Console.WriteLine($"Folder Index: {folderIndex}, File Index: {fileIndex}");
                    float playtime = float.Parse(playtimetxt.Text);
                    Task.Run(() => PlayWav(folderIndex, fileIndex, playtime));

                    RecordProcess(textValue);
                }
                else
                {
                    // 오류 처리 (예상치 못한 값의 경우)
                    Console.WriteLine("텍스트 형식이 잘못되었습니다.");
                }


            }

            if (e.Key == Key.Space)
            {
                RecordProcess("쉼표");
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Left)
            {
                if (playtimeslider.Value > 31)
                {
                    playtimeslider.Value = 32;
                }
                else
                {
                    playtimeslider.Value = playtimeslider.Value + 1;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.Right)
            {
                if (playtimeslider.Value < 2)
                {
                    playtimeslider.Value = 1;
                }
                else
                {
                    playtimeslider.Value = playtimeslider.Value - 1;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Left)
            {
                if (playintervalslider.Value < 20)
                {
                    playintervalslider.Value = 10;
                }
                else
                {
                    playintervalslider.Value = playintervalslider.Value - 10;
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.Right)
            {
                if (playintervalslider.Value > 390)
                {
                    playintervalslider.Value = 400;
                }
                else
                {
                    playintervalslider.Value = playintervalslider.Value + 10;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F1)
            {
                playtimeslider.Value = 2;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F2)
            {
                playtimeslider.Value = 4;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F3)
            {
                playtimeslider.Value = 8;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F4)
            {
                playtimeslider.Value = 16;
            }
            else if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F5)
            {
                playtimeslider.Value = 32;
            }

            if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.F1)
            {
                playintervalslider.Value = float.Parse(txtf1.Text);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.F2)
            {
                playintervalslider.Value = float.Parse(txtf2.Text);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.F3)
            {
                playintervalslider.Value = float.Parse(txtf3.Text);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.F4)
            {
                playintervalslider.Value = float.Parse(txtf4.Text);
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift && e.Key == Key.F5)
            {
                playintervalslider.Value = float.Parse(txtf5.Text);
            }

        }

        private void playtimeslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            playtimeprocces();
        }

        private void playtimeprocces()
        {
            if (playtimeslider == null || playtimetxt == null)
            {
                return;  // 객체가 null인 경우 함수 종료
            }

            PlayTimeNotesTxt.Text = playtimeslider.Value.ToString();
            /* 1-32 BPM = playintervalslider 1~400 bpm 
                60 bpm 이면 1초   60s / 60bpm
                400 bpm = 0.15초  60s / 400bpm

                곡의 템포가 60 BPM이면 1박자는 1초이고, 120 BPM이면 1박자는 0.5초가 됩니다.
                이를 기반으로 음의 길이를 초 단위로도 계산할 수 있습니다.
                온음표(Whole Note): 4박자                            1   bpm ms x4  /1
                2분음표(Half Note): 2박자                            2   bpm ms x2  /2
                4분음표(Quarter Note): 1박자    1초      0.15초      4   bpm ms x1  /4
                8분음표(Eighth Note): 1 / 2박자                      8   bpm ms /2  /8
                16분음표(Sixteenth Note): 1 / 4박자                  16  bpm ms /4  /16
                32분음표(Thirty - second Note): 1 / 8박자            32  bpm ms /8  /32  playintervaltxt
            */
            playtimetxt.Text = (int.Parse(playintervaltxt.Text) * 4 / int.Parse(playtimeslider.Value.ToString())).ToString();
        }

        private void playintervalslider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (playintervalslider == null || playintervaltxt == null)
            {
                return;  // 객체가 null인 경우 함수 종료
            }

            playintervaltxt.Text = playintervalslider.Value.ToString();
            BPMtxt.Text = playintervalslider.Value.ToString();
            //ms 계산해서 넣기 60 ~ 400  60
            playintervaltxt.Text = (60000 / int.Parse(playintervalslider.Value.ToString())).ToString();

            playtimeprocces();
        }

        private void wavtree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            wavtree_selectedChangeFunction();
        }

        private void wavtree_selectedChangeFunction()
        {
            addinsBTN.IsEnabled = true;
            LoadSettingFileNameBtn.IsEnabled = true;
            //wavtree_SelectedItemChanged
            if (wavtree.SelectedItem != null)
            {
                TreeViewItem selectedItem = wavtree.SelectedItem as TreeViewItem;
                if (selectedItem != null && selectedItem.Tag != null)
                {
                    // 선택한 아이템의 부모 폴더 (트리에서 상위 아이템)
                    TreeViewItem parentItem = selectedItem.Parent as TreeViewItem;
                    string isfolder = "";
                    if (selectedItem.Tag is FolderInfo folderInfo3)
                    {
                        isfolder = folderInfo3.FolderPath;
                    }

                    if (!IsDirectory(isfolder))
                    {
                        if (parentItem != null)
                        {
                            int fileIndex = 0;
                            if (selectedItem.Tag is FolderInfo folderInfo)
                            {
                                fileIndex = folderInfo.Index;
                            }

                            int folderIndex2 = 0;
                            if (parentItem.Tag is FolderInfo folderInfo2)
                            {
                                folderIndex2 = folderInfo2.Index;
                            }

                            // 인덱스를 텍스트박스에 출력 (몇번째 폴더의 몇번째 파일)
                            midindextxt.Text = $"{folderIndex2},{fileIndex}";

                            string fullPath = "";

                            if (parentItem.Tag is FolderInfo folderInfo4)
                            {
                                fullPath = folderInfo4.FolderPath;
                            }
                            string[] pathfactor = fullPath.Split('\\');

                            InstrumentTxt.Text = $"{folderIndex2},{pathfactor[pathfactor.Length - 1]}";
                            float playtime = float.Parse(playtimetxt.Text);
                            Task.Run(() => PlayWav(folderIndex2, fileIndex, playtime));
                        }
                    }
                }
            }
        }

        private void logtxt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 클릭한 위치의 캐릭터 인덱스 가져오기
            int charIndex = logtxt.GetCharacterIndexFromPoint(e.GetPosition(logtxt), true);

            // 캐릭터 인덱스로부터 줄 인덱스 계산
            int lineIndex = logtxt.GetLineIndexFromCharacterIndex(charIndex);

            // 해당 줄의 첫 번째와 마지막 문자 인덱스 가져오기
            int lineStart = logtxt.GetCharacterIndexFromLineIndex(lineIndex);
            int lineEnd = logtxt.GetLineLength(lineIndex) + lineStart;

            // 해당 줄 선택
            logtxt.Select(lineStart, lineEnd - lineStart);
        }
        private async void PlayBTN_Click(object sender, RoutedEventArgs e)
        {
            playNotes();
        }

        private void stopBTN_Click(object sender, RoutedEventArgs e)
        {
            playbool = true;
        }

        private async void LoadFilesBTN_Click(object sender, RoutedEventArgs e)
        {
            await Task.Run(() => disposetask());
            //selectedon = false;

            await Dispatcher.InvokeAsync(() =>
            {
                LoadFilesBTN.IsEnabled = false;
            });

            //prepareMusicNotes();
            //PlayBTN.IsEnabled = true;
            Task.Run(() => prepareMusicNotes());
        }

        private async void playNotes()
        {
            try
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    PlayBTN.IsEnabled = false;
                    playbool = false;
                    LoadFilesBTN.IsEnabled = false;
                });
                int playlinecnt = 0;
                int playColumnCnt = 0;
                int playColumnMaxCnt = datagridview1.Columns.Count - 2;
                string totalline = datagridview1.Items.Count.ToString();

                foreach (var item in playbackList)
                {
                    /*
                    playColumnCnt++;
                    if (playColumnCnt > playColumnMaxCnt - 1)
                    {
                        playColumnCnt=0;
                        playlinecnt++;
                    }
                    */


                    await Dispatcher.InvokeAsync(() =>
                    {
                        int rowIndex = playlinecnt;
                        if (rowIndex >= 0 && rowIndex < datagridview1.Items.Count)
                        {
                            datagridview1.SelectedCells.Clear();
                            for (int columnIndex = 0; columnIndex < datagridview1.Columns.Count; columnIndex++)
                            {
                                datagridview1.SelectedCells.Add(new DataGridCellInfo(datagridview1.Items[rowIndex], datagridview1.Columns[columnIndex]));
                            }

                            // Ensure the selected cell is scrolled into view.
                            if (datagridview1.Items.Count > (rowIndex + 6) && rowIndex > 5)
                            {
                                datagridview1.ScrollIntoView(datagridview1.Items[rowIndex + 5]);
                            }
                            else if (rowIndex < 5)
                            {
                                datagridview1.ScrollIntoView(datagridview1.Items[rowIndex]);
                            }
                            else if (datagridview1.Items.Count < (rowIndex + 6))
                            {
                                datagridview1.ScrollIntoView(datagridview1.Items[datagridview1.Items.Count - 1]);
                            }

                        }
                    });


                    float interval2 = item.Interval;
                    WaveOutEvent waveOut = item.WaveOut;
                    MemoryStream MS = item.MS;
                    float playTime = item.PlayTime;
                    float internalvolume = item.Volumes;

                    if (!playbool)
                    {
                        await Task.Delay((int)interval2);

                        Task.Run(() => PlayPreparedAudioAsync(waveOut, playTime, MS, internalvolume));
                    }
                    else
                    {
                        await Task.Run(() => PlayPreparedAudioAsync(waveOut, playTime, MS, 0.0f));
                    }

                    if (interval2 != 0) { playlinecnt++; }

                    await Dispatcher.InvokeAsync(() =>
                    {
                        currentplaylinetxt.Text = playlinecnt.ToString() + " / " + totalline;
                    });


                }
                playbool = false;
                await Dispatcher.InvokeAsync(() =>
                {
                    LoadFilesBTN.IsEnabled = true;
                });

            }
            catch (Exception ex)
            {
                scalesettingtxt.AppendText(ex.Message);
            }

            await Dispatcher.InvokeAsync(() =>
            {
                datagridview1.UnselectAll();
            });
        }

        private async void prepareMusicNotes()
        {
            string[] playlines = { "" };
            await Dispatcher.InvokeAsync(() =>
            {
                playlines = logtxt.Text.Split('\n');
            });
            //string[] playlines = logtxt.Text.Split('\n');         // 콤마로 구분
            int playlinecnt = 0;
            string totallines = (playlines.Count() - 1).ToString();
            playbackList.Clear();
            foreach (string playline in playlines)
            {
                if (playline.Length > 3)
                {
                    string[] factors = playline.Split(':');
                    float interval = float.Parse(factors[0]);

                    if (factors.Length > 1)
                    {
                        int i = 0;
                        //foreach (string factor in factors)
                        //{
                        string[] midis = factors[1].Split('|');

                        foreach (string midi in midis)
                        {
                            string[] midisub = midi.Split(',');
                            if (midisub.Length >= 2)
                            {
                                try
                                {
                                    int folderIndex = int.Parse(midisub[0]);     // 첫 번째 값: 폴더 인덱스
                                    int fileIndex = int.Parse(midisub[1]);       // 두 번째 값: 파일 인덱스
                                    float playtimemilli = float.Parse(midisub[2]);       // 두 번째 값: 파일 인덱스

                                    //if (midisub.Length >= 4)
                                    //{
                                    //    volume = float.Parse(midisub[3]);       // 두 번째 값: 파일 인덱스
                                    //}
                                    //Thread playThread = new Thread(() => PlayWav(folderIndex, fileIndex, playtimemilli));
                                    //playThread.Start();
                                    //Task.Run(() => PlayWav(folderIndex, fileIndex, playtimemilli));

                                    //if (folderIndex < wavFiles.Count && fileIndex < wavFiles[folderIndex].Count)
                                    //{
                                    // 리스트에 항목 추가 예시
                                    MemoryStream memoryStream = new MemoryStream();
                                    WaveOutEvent waveOut = new WaveOutEvent();

                                    if (folderIndex >= 0 && fileIndex >= 0) waveOut = PrepareAudio(folderIndex, fileIndex, out memoryStream); ;

                                    //if (waveOut != null)
                                    //{
                                    playbackList.Add(new AudioPlaybackInfo
                                    {
                                        Interval = interval,
                                        WaveOut = waveOut,//PrepareAudio(folderIndex, fileIndex),  // Now assign WaveOut
                                        MS = memoryStream,  // Now assign MS
                                        PlayTime = playtimemilli,
                                        Volumes = volume
                                    });
                                    //}
                                    //}

                                    //PlayWav(folderIndex, fileIndex, playtimemilli);
                                    //currentplaylinetxt.AppendText(midi.ToString());
                                    interval = float.Parse("0");
                                    i++;
                                }
                                catch (Exception ex)
                                {
                                    await Dispatcher.InvokeAsync(() =>
                                    {
                                        scalesettingtxt.AppendText(ex.ToString());
                                    });
                                }

                            }
                        }
                        //await Task.Delay(TimeSpan.FromMilliseconds(interval));
                        //}
                    }
                }
                playlinecnt++;
                // Update the text on the UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    currentplaylinetxt.Text = playlinecnt.ToString() + " / " + totallines;
                });
            }

            await Dispatcher.InvokeAsync(() =>
            {
                PlayBTN.IsEnabled = true;
                /*
                scalesettingtxt.AppendText("playbacklist counts : " + playbackList.Count().ToString() + "\r\n");
                foreach(AudioPlaybackInfo temp in playbackList)
                {
                    scalesettingtxt.AppendText($"{temp.Interval},{temp.WaveOut},{temp.PlayTime} \r\n");
                }*/
            });


            playNotes();
        }

        private void SimpleSetNumkey_Click(object sender, RoutedEventArgs e)
        {
            simplesetting();
        }
        private void simplesetting()
        {
            string[] index = midindextxt.Text.Split(',');
            string[] temp = { "c4", "d4", "e4", "f4", "g4", "a4", "b4", "c5", "d5" };
            string[] temp2 = { "c4", "d4", "e4", "f4", "g4", "a4", "b4", "c5", "d5" };

            int parsedIndex = int.Parse(index[0]);
            int formax = wavFiles[parsedIndex].Count() - 1;
            Regex regex = new Regex("[a-zA-Z]{1,2}\\d+");

            for (int i = 0; i < formax; i++)
            {
                int j = 0;
                Match match = regex.Match(wavFiles[parsedIndex][i]);
                if (match.Success)
                {
                    foreach (string tempcontain in temp)
                    {
                        if (match.Value.ToLower() == tempcontain)
                        {
                            temp2[j] = $"{parsedIndex},{i}";
                        }
                        j++;
                    }
                }
            }


            // 파일명을 텍스트박스에 표시 (최대 9개까지만 표시)
            for (int i = 0; i < fileTextBoxes.Length; i++)
            {
                if (i < wavFiles[int.Parse(index[0])].Count)
                {
                    if (temp[i] == temp2[i])
                    {
                        fileTextBoxes[i].Text = index[0] + "," + i.ToString();
                    }
                    else
                    {
                        fileTextBoxes[i].Text = temp2[i];//index[0] + "," + i.ToString();
                    }

                }
                else
                {
                    fileTextBoxes[i].Text = string.Empty;
                }

            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ScaleTxttoColumnBTN.IsEnabled = true;

            initializeingGrid();

            GridAddColumn();


        }

        private void GridAddColumn()
        {
            // 새로운 열 생성 새로운 악기추가 왼쪽에서 미디파일 클릭하면 바로위 미디인데스값 표시 오른쪽위 InstrumentTxt 값 가져다 악기칼럼추가
            DataGridTextColumn newColumn = new DataGridTextColumn();

            if (InstrumentTxt.Text.Split(',').Count() == 2)
            {
                // 텍스트 박스에서 헤더를 가져오기
                string header = InstrumentTxt.Text.Split(',')[1].Trim(); // 필요한 인덱스를 사용하고 공백 제거

                newColumn.Header = header; // 헤더 설정
                newColumn.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridview1.Columns.Count));
                //newColumn.Binding = new System.Windows.Data.Binding(header); // 데이터 모델의 속성과 바인딩
                IndexofCollumn.Text = datagridview1.Columns.Count.ToString();

                // DataGrid에 열 추가
                datagridview1.Columns.Add(newColumn);
            }

            if (datagridview1.Columns.Count > 2)
            {
                foreach (var item in Intervals)
                {
                    item.Values.Add(""); // 기본값으로 빈 문자열 추가
                }
            }

        }

        private void initializeingGrid()
        {
            // "interval" 열이 이미 존재하는지 확인
            bool columnExists = false;
            foreach (DataGridTextColumn column in datagridview1.Columns)
            {
                if (column.Header.ToString() == "index")
                {
                    columnExists = true;
                    break;
                }
            }

            // 열이 없으면 새로 추가
            if (!columnExists)
            {
                // ObservableCollection 초기화
                Intervals = new ObservableCollection<IntervalData>();
                datagridview1.ItemsSource = Intervals; // DataGrid에 바인딩

                //Intervals.Add(new IntervalData { Interval = 220 });

                DataGridTextColumn intervalColumn = new DataGridTextColumn();
                intervalColumn.Header = "index"; // 열 이름 설정
                intervalColumn.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridview1.Columns.Count));// new System.Windows.Data.Binding(Intervals); // 데이터 모델의 속성과 바인딩
                                                                                                                                    // DataGrid의 첫 번째 위치에 열 추가
                datagridview1.Columns.Add(intervalColumn);

                DataGridTextColumn intervalColumn2 = new DataGridTextColumn();
                intervalColumn2.Header = "interval"; // 열 이름 설정
                intervalColumn2.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridview1.Columns.Count));// new System.Windows.Data.Binding(Intervals); // 데이터 모델의 속성과 바인딩
                                                                                                                                     // DataGrid의 첫 번째 위치에 열 추가
                datagridview1.Columns.Add(intervalColumn2);

            }
        }


        public ObservableCollection<IntervalData> Intervals { get; set; }
        public ObservableCollection<IntervalData> Intervalsetting { get; set; }

        public ObservableCollection<IntervalData> keysetting { get; set; }

        public class IntervalData
        {
            //public int Interval { get; set; } // 기본적인 Interval 값
            public List<string> Values { get; set; } // 행렬 데이터를 담을 리스트

            public IntervalData()
            {
                Values = new List<string>(); // 리스트 초기화
            }
        }

        private void Addindex(List<string> noteValues)
        {
            int beforeitemscount = Intervals.Count();
            var intervalColumn = datagridview1.Columns[0];
            if (intervalColumn != null)
            {
                // 220의 값을 10줄 추가 20 10 20 30
                for (int i = beforeitemscount; i < (int)noteValues.Count() + beforeitemscount; i++)
                {
                    Intervals.Add(new IntervalData { Values = new List<string> { (i).ToString() } });
                    //Intervals.Add(new IntervalData { Values = playtimeslider.Value.ToString() });
                    //Intervals.Add(new IntervalData { Values = new List<string> { playtimeslider.Value.ToString() } });
                    //datagridview1.Items.Add(new { Interval = "200" }); // "interval" 열에 값 설정

                }
            }
            else
            {
                System.Windows.MessageBox.Show("Interval column does not exist.");
            }


            int i2 = 0;
            string interval = playintervaltxt.Text;
            // interval 에는 증가된 아이탬이 이미 있다. 
            foreach (var item in Intervals)
            {
                if (beforeitemscount <= i2)
                {
                    if (item.Values.Count < datagridview1.Columns.Count)  //datagridview1.Columns.Count()
                    {
                        //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                        item.Values.Add(interval); // 초기 값 설정

                    }
                }
                i2++;
            }
        }

        private void ModAddindex(int noteCNT, int rows, int column)
        {
            int beforeitemscount = Intervals.Count();
            var intervalColumn = datagridview1.Columns[0];
            int totalincreaseCnt = noteCNT;

            if (intervalColumn != null)
            {
                // 100   9+99 108      10   10 5
                if ((totalincreaseCnt + rows) > beforeitemscount)
                {
                    for (int i = beforeitemscount; i <= (totalincreaseCnt + rows); i++)
                    {
                        Intervals.Add(new IntervalData { Values = new List<string> { (i).ToString() } });
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Interval column does not exist.");
            }


            int i2 = 0;
            string interval = playintervaltxt.Text;
            // interval 에는 증가된 아이탬이 이미 있다. 
            foreach (var item in Intervals)
            {
                if (beforeitemscount <= i2)
                {
                    if (item.Values.Count < datagridview1.Columns.Count)  //datagridview1.Columns.Count()
                    {
                        item.Values.Add(interval); // 초기 값 설정
                        //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                    }
                }
                i2++;
            }
            i2 = 0;
            for (int j = 2; j <= datagridview1.Columns.Count(); j++)
            {
                foreach (var item in Intervals)
                {
                    if (beforeitemscount <= i2)
                    {
                        //if (item.Values.Count < datagridview1.Columns.Count)  //datagridview1.Columns.Count()
                        //{
                        item.Values.Add(""); // 초기 값 설정
                                             //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                                             //}
                    }
                    i2++;
                }
                i2 = 0;
            }
        }
        private void initializeindex(List<string> noteValues)
        {
            if (Intervals.Count < noteValues.Count())
            {
                // 220의 값을 10줄 추가
                for (int i = Intervals.Count; i < (int)noteValues.Count(); i++)
                {
                    //Intervals.Add(new IntervalData { Values = playtimeslider.Value.ToString() });
                    //Intervals.Add(new IntervalData { Values = new List<string> { playtimeslider.Value.ToString() } });
                    Intervals.Add(new IntervalData { Values = new List<string> { i.ToString() } });
                    //Intervals.Add(new IntervalData { Values = new List<string> { playtimeslider.Value.ToString() } });
                    //datagridview1.Items.Add(new { Interval = "200" }); // "interval" 열에 값 설정
                }
            }
        }

        private void Addinterval(List<string> noteValues)
        {
            int beforeitemscount = Intervals.Count();
            int i2 = 0;

            // interval 에는 증가된 아이탬이 이미 있다. 
            foreach (var item in Intervals)
            {
                if (beforeitemscount <= i2)
                {
                    if (item.Values.Count < datagridview1.Columns.Count)  //datagridview1.Columns.Count()
                    {
                        //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                        item.Values.Add(playtimeslider.Value.ToString()); // 초기 값 설정

                    }
                }
                i2++;
            }

        }

        private void ScaleTxttoColumnBTN_Click(object sender, RoutedEventArgs e)
        {
            ScaleTxttoColumnBTN_Click_Function();
        }

        private void ScaleTxttoColumnBTN_Click_Function()
        {
            ModScaleToExcelBTN.IsEnabled = true;
            ClearExcel.IsEnabled = true;
            intervalchangeBTN.IsEnabled = true;

            string dividedscaltxt = ScaleTxt.Text;


            if (ScaleTxt.Text.Contains("+"))
            {
                string[] dividedscaletxt = dividedscaltxt.Split('+');

                scaletxttoexcel(dividedscaletxt[0]);

                for (int i = 1; i < dividedscaletxt.Length; i++)
                {
                    GridAddColumn();

                    int rightmostColumnIndex = datagridview1.Columns.Count - 1;
                    datagridview1.CurrentCell = new DataGridCellInfo(datagridview1.Items[0], datagridview1.Columns[rightmostColumnIndex]);
                    datagridview1.SelectedCells.Clear();
                    datagridview1.SelectedCells.Add(datagridview1.CurrentCell);

                    ModScaleFunction(dividedscaletxt[i]);
                }
                //추가되는 악보만큼
                //인스트루먼트 추가하고
                //추가된 인스트루먼트 맨위를 선택하고 추가된 악보의 음표를 엑셀에 넣기

            }
            else
            {
                scaletxttoexcel(dividedscaltxt);
            }
        }

        private void scaletxttoexcel(string dividedscaltxt)
        {
            int columnIndex;
            bool isValidIndex = int.TryParse(IndexofCollumn.Text, out columnIndex); // 문자열을 정수로 변환 시도
            // 텍스트 박스에서 값 가져오기
            int interval = 0;
            bool intervaltxtparsesuccess = int.TryParse(playintervaltxt.Text, out interval);
            List<string> noteValues = GetNoteValuesFromTextBox(dividedscaltxt);
            int beforeItemscount = Intervals.Count();

            if (datagridview1.Columns.Count > 0)
            {
                Addindex(noteValues);
                //Addinterval(noteValues);
            }
            else
            {
                initializeingGrid();
                Addindex(noteValues);
                //Addinterval(noteValues);

                System.Windows.MessageBox.Show("Must Need Add Instrument Column.");
                return;
            }


            int i2 = 0;

            /*
            // interval 에는 증가된 아이탬이 이미 있다. 
            foreach (var item in Intervals)
            {
                if (beforeItemscount <= i2)
                {
                    if (item.Values.Count < datagridview1.Columns.Count)  //datagridview1.Columns.Count()
                    {
                        //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                        //item.Values.Add("New Value add " + (i2)); // 초기 값 설정
                        //item.Values[columnIndex] = "New Value add " + (i2);
                        //item.Values[0] = "New Value add " + (i2);
                        item.Values.Add("New Value add " + (i2));
                    }
                }
                i2++;
            }
            */

            i2 = 0;
            int i3 = 0;
            foreach (var item in Intervals)
            {
                //item.Values.Add("New Value add " + (i2)); // 초기 값 설정
                // 두 번째 열이 존재하지 않으면 먼저 리스트의 크기를 확장
                //if (item.Values.Count >= 2)
                //{
                // 두 번째 열(인덱스 1)에 값 할당
                //===============================  앞으로 옮김
                //int columnIndex;
                //bool isValidIndex = int.TryParse(IndexofCollumn.Text, out columnIndex); // 문자열을 정수로 변환 시도

                // if (0 < int.Parse(IndexofCollumn.Text) && int.Parse(IndexofCollumn.Text) < datagridview1.Columns.Count())

                if (beforeItemscount <= i2)
                {

                    if (isValidIndex && columnIndex >= 0 && columnIndex < datagridview1.Columns.Count)
                    {
                        while (item.Values.Count <= columnIndex)
                        {
                            item.Values.Add(""); // 기본값으로 빈 문자열 추가
                        }

                        // noteValues 배열에서 값 할당
                        if (i2 < (noteValues.Count + beforeItemscount)) // noteValues의 길이를 초과하지 않도록 체크
                        {
                            item.Values[columnIndex] = noteValues[i3];
                            i3++;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Note values exceed available items.");
                            break;
                        }
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Put in Index of Column.");
                        break;
                    }
                }
                i2++;
                //}
            }

            /*
            foreach (var noteValue in noteValues)
            {
                datagridview1.Items.Add(new { Interval = interval, note01 = noteValue });
            }

            // 데이터 바인딩 업데이트 (필요 시)
            datagridview1.Items.Refresh();
            */
        }

        // 도,레,미,파,솔,라,시,도 데이터를 가져와 숫자로 변환하는 메서드
        List<string> GetNoteValuesFromTextBox(string dividedscaletxt)
        {
            var noteValues = new List<string>();
            string stringToRemove = "\r\n";
            dividedscaletxt = dividedscaletxt.Replace(stringToRemove, "");

            // 정규식을 사용하여 "a숫자" 패턴 찾기
            string pattern = @"x(\d+)";

            // Regex.Replace를 사용하여 b 값을 찾아서 처리
            string result = Regex.Replace(dividedscaletxt, pattern, match =>
            {
                // b 값 (즉, 숫자 부분)을 추출
                int b = int.Parse(match.Groups[1].Value);

                // ','를 b-1 개만큼 추가
                string commas = new string(',', b - 1);

                // 결과로 "a" + b 개수만큼의 ',' 문자열 반환
                return "x" + b.ToString() + commas;
            });

            result = result.Replace(",&", "&");

            string[] notes = result.Split(',');

            for (int i = 0; i < notes.Length; i++)
            {
                noteValues.Add(notes[i]); // 1, 2, 3, ...으로 치환
            }

            return noteValues;
        }

        private void datagridview1_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if (datagridview1.SelectedCells.Count > 0)
            {
                // 첫 번째 선택된 셀을 가져옴
                DataGridCellInfo selectedCell = datagridview1.SelectedCells[0];

                // 셀이 속한 열을 가져옴
                DataGridColumn column = selectedCell.Column;

                // 열의 인덱스를 계산
                int columnIndex = datagridview1.Columns.IndexOf(column);

                // 텍스트박스에 열 인덱스를 표시
                SelectedInstrumentColumnTXT.Text = columnIndex.ToString();
                IndexofCollumn.Text = columnIndex.ToString();
                // 셀이 속한 행의 데이터를 가져옴
                var rowData = selectedCell.Item;

                // 행의 인덱스 구하기
                int rowIndex = datagridview1.Items.IndexOf(rowData);

                // 텍스트박스에 행 인덱스를 표시
                SelectedInstrumentRowTXT.Text = rowIndex.ToString();

                //datagridview1.UnselectAllCells();

                Column_clear_BTN.IsEnabled = true;

                focusoutbtn.Focus();
            }
        }

        private void intervalchangeBTN_Click(object sender, RoutedEventArgs e)
        {
            string interval = playintervaltxt.Text;

            if (datagridview1.SelectedCells.Count <= 0)
            {
                foreach (var item in Intervals)
                {
                    item.Values[1] = interval;
                }
                datagridview1.Items.Refresh();

            }
            else
            {
                // 선택된 셀이 있을 때만 처리
                foreach (var selectedCell in datagridview1.SelectedCells)
                {
                    // 선택된 셀의 정보
                    DataGridCellInfo cellInfo = selectedCell;

                    // 선택된 셀의 행 정보 가져오기
                    var row = cellInfo.Item as IntervalData;  // Item을 IntervalData로 캐스팅

                    if (row != null)
                    {
                        // IntervalData 객체의 Values 리스트에서 1번째 값을 업데이트
                        row.Values[1] = interval.ToString();  // 필요한 값을 할당

                        // 데이터 그리드를 강제로 새로고침해서 값이 반영되도록 함
                        datagridview1.Items.Refresh();
                    }
                }
                datagridview1.UnselectAll();
            }
        }

        private void ModScaleToExcelBTN_Click(object sender, RoutedEventArgs e)
        {
            if (!ScaleTxt.Text.Contains("+"))
            {
                ModScaleFunction("null");
            }
            else
            {
                System.Windows.MessageBox.Show("This Scal txt array have lots hands");
            }

        }

        private void ModScaleFunction(string scaleTxtdivided)
        {
            if (scaleTxtdivided == "null")
            {
                scaleTxtdivided = ScaleTxt.Text;
            }

            List<string> noteValues = GetNoteValuesFromTextBox(scaleTxtdivided);

            int noteCNT = noteValues.Count;
            int rows = int.Parse(SelectedInstrumentRowTXT.Text);
            int column = int.Parse(SelectedInstrumentColumnTXT.Text);
            int i = 0;

            if (rows + noteValues.Count > Intervals.Count)
            {
                ModAddindex(noteCNT, rows, column);
            }
            datagridview1.Items.Refresh();

            //foreach (string note in noteValues)
            foreach (var item in Intervals)
            {
                if (i >= rows && i < (noteCNT + rows))
                {
                    item.Values[column] = noteValues[i - rows];// "item.Values[" +column.ToString()+","+ (i - rows).ToString() + "]";// 
                }
                i++;
            }
            datagridview1.Items.Refresh();
        }

        private void scaleloading()
        {
            datagridscaleSetting.AutoGenerateColumns = false;

            //Intervalsetting.Clear();
            // ObservableCollection 초기화
            Intervalsetting = new ObservableCollection<IntervalData>();
            datagridscaleSetting.ItemsSource = Intervalsetting; // DataGrid에 바인딩

            //Intervals.Add(new IntervalData { Interval = 220 });

            DataGridTextColumn intervalColumn = new DataGridTextColumn();
            intervalColumn.Header = "code"; // 열 이름 설정
            intervalColumn.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridscaleSetting.Columns.Count));// new System.Windows.Data.Binding(Intervals); // 데이터 모델의 속성과 바인딩
                                                                                                                                       // DataGrid의 첫 번째 위치에 열 추가
            datagridscaleSetting.Columns.Add(intervalColumn);
            datagridscaleSetting.Items.Refresh();

            DataGridTextColumn intervalColumn2 = new DataGridTextColumn();
            intervalColumn2.Header = "scalecode"; // 열 이름 설정
            intervalColumn2.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridscaleSetting.Columns.Count));// new System.Windows.Data.Binding(Intervals); // 데이터 모델의 속성과 바인딩
                                                                                                                                        // DataGrid의 첫 번째 위치에 열 추가
            datagridscaleSetting.Columns.Add(intervalColumn2);
            datagridscaleSetting.Items.Refresh();

            DataGridTextColumn intervalColumn3 = new DataGridTextColumn();
            intervalColumn3.Header = "index"; // 열 이름 설정
            intervalColumn3.Binding = new System.Windows.Data.Binding(string.Format("Values[{0}]", datagridscaleSetting.Columns.Count));// new System.Windows.Data.Binding(Intervals); // 데이터 모델의 속성과 바인딩
                                                                                                                                        // DataGrid의 첫 번째 위치에 열 추가
            datagridscaleSetting.Columns.Add(intervalColumn3);
            datagridscaleSetting.Items.Refresh();
        }

        private void datagridscaleSetting_Initialized(object sender, EventArgs e)
        {
            scaleloading();
        }

        private void ClearExcel_Click(object sender, RoutedEventArgs e)
        {
            ExcelClear();

        }

        private void ExcelClear()
        {
            if (Intervals != null)
            {
                Intervals.Clear();
            }
            datagridview1.ItemsSource = null;
            datagridview1.Items.Clear();
            datagridview1.Columns.Clear();

            intervalchangeBTN.IsEnabled = false;
            ScaleTxttoColumnBTN.IsEnabled = false;
            ModScaleToExcelBTN.IsEnabled = false;
            ClearExcel.IsEnabled = false;
        }

        private void ScaleSettingBTN_Click(object sender, RoutedEventArgs e)
        {
            ConvertorBTN.IsEnabled = true;
            PrepareMusicBTN.IsEnabled = true;
            RecordBtn.IsEnabled = true;

            ScaleSettingProcess();
        }

        private void ScaleSettingProcess()
        {
            Intervalsetting.Clear();
            Intervalsetting = null;
            datagridscaleSetting.ItemsSource = null;
            datagridscaleSetting.Items.Clear();
            datagridscaleSetting.Columns.Clear();
            scaleloading();

            //도=c3=1,33_레=d3=1,41_
            string[] noteValues = scalesettingTXT.Text.Split('_');


            int beforeitemscount = Intervalsetting.Count();
            // '='로 구분된 값을 저장할 리스트 생성
            List<string[]> splitResults = new List<string[]>();

            foreach (string notes in noteValues)
            {
                string[] notesTXT = notes.Split('=');
                splitResults.Add(notesTXT); // 결과를 리스트에 저장

                Intervalsetting.Add(new IntervalData { Values = new List<string> { notesTXT[0] } });
            }

            int i2 = 0;
            foreach (var item in Intervalsetting)
            {
                string[] storedNotesTXT = splitResults[i2]; // 저장된 'notesTXT' 값 가져오기
                i2++;
                if (storedNotesTXT.Count() > 2)
                {
                    //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                    item.Values.Add(storedNotesTXT[1]); // 초기 값 설정
                }

            }

            i2 = 0;
            foreach (var item in Intervalsetting)
            {
                string[] storedNotesTXT = splitResults[i2]; // 저장된 'notesTXT' 값 가져오기
                i2++;
                if (storedNotesTXT.Count() > 2)
                {
                    //item.Values[1].Add = "0," + i2+",200"; // 초기 값 설정
                    item.Values.Add(storedNotesTXT[2]); // 초기 값 설정
                }
            }
        }

        private void PrepareMusicBTN_Click(object sender, RoutedEventArgs e)
        {
            preparemusicbtn_Function();

        }

        private void preparemusicbtn_Function()
        {
            LoadFilesBTN.IsEnabled = true;

            logtxt.Text = "";

            foreach (var item in Intervals)
            {
                string temp = item.Values[1] + ":"; // + item.Values[2].ToString() + "|";
                for (int i = 2; i < item.Values.Count; i++)
                {
                    //temp += item.Values[i] + ",";

                    // 정규 표현식 패턴: "x" 뒤에 숫자가 오는 패턴
                    string pattern = @"x(\d+)";

                    // 정규 표현식 사용
                    Match match = Regex.Match(item.Values[i], pattern);

                    if (match.Success)
                    {
                        // match.Groups[1]에서 숫자 부분만 추출
                        int extractedNumber = int.Parse(match.Groups[1].Value);
                        int playtime = int.Parse(playtimetxt.Text) * extractedNumber;

                        if (item.Values[i].IndexOf("쉼표") != -1)
                        {
                            item.Values[i] = item.Values[i].Replace("쉼표", "0,0");
                        }

                        temp += item.Values[i].Replace("x" + extractedNumber.ToString(), "") + "," + playtime.ToString() + "|";
                    }
                    else
                    {
                        string playtime = playtimetxt.Text;
                        if (item.Values[i] == "")
                        {
                            temp += item.Values[i] + "0,0," + playtime + "|";
                        }
                        else if (item.Values[i] == "쉼표")
                        {
                            temp += "0,0," + playtime + "|";
                        }
                        else
                        {
                            temp += item.Values[i] + "," + playtime + "|";
                        }

                    }

                    if (temp.Contains('&'))
                    {
                        string replacement = ',' + item.Values[1] + '|';
                        temp = temp.Replace("&", replacement);
                    }

                    // + item.Values[1] + "|";  //item.Values[i] 여기에 x1~x5 가 있으면 뒤의 item.Values[1] 값을 곱하기x1~5해주는 코드
                }

                logtxt.AppendText(temp + "\r\n");
            }

            foreach (var item in Intervalsetting)
            {
                if (item.Values.Count > 2)
                {
                    string stringToRemove = item.Values[0].ToLower();
                    logtxt.Text = logtxt.Text.ToLower();
                    logtxt.Text = logtxt.Text.Replace(stringToRemove, item.Values[2]);
                }
            }
        }

        private void VolumeCtrl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {


            if (VolumeCtrl != null && VolumeTXT != null)
            {
                volume = (float.Parse(VolumeCtrl.Value.ToString()) / 100);

                VolumeTXT.Text = volume.ToString();
            }

            //volume = 0.3f;
        }

        private void VolumeTXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            float internalvolume = float.Parse(VolumeTXT.Text);
            if (0.0f < internalvolume && internalvolume <= 1.0f)
            {
                VolumeCtrl.Value = (double)(internalvolume * 100);
            }
        }

        private void Column_clear_BTN_Click(object sender, RoutedEventArgs e)
        {
            Column_clear_BTN.IsEnabled = false;

            int selectedcolsindex = int.Parse(SelectedInstrumentColumnTXT.Text);

            foreach (var item in Intervals)
            {
                if (selectedcolsindex <= item.Values.Count())
                {
                    item.Values.RemoveAt(selectedcolsindex); //item.Values[selectedcolsindex] = "";
                }
            }
            datagridview1.Columns.RemoveAt(selectedcolsindex);
            datagridview1.Items.Refresh();
        }

        private void scalesettingTXT_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ScaleSetHelp.Visibility = Visibility.Visible;
        }

        private void scalesettingTXT_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ScaleSetHelp.Visibility = Visibility.Hidden;
        }

        private void datagridscaleSetting_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EasySettingTooltip.Visibility = Visibility.Visible;
        }

        private void datagridscaleSetting_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            EasySettingTooltip.Visibility = Visibility.Hidden;
        }

        private void ConvertorBTN_Click(object sender, RoutedEventArgs e)
        {
            //도=c3=1,33_레=d3=1,41_
            string[] noteValues = scalesettingTXT.Text.Split('_');

            int instrumentIndex = int.Parse(midindextxt.Text.Split(',')[0]);
            string[] wavfile = wavFiles[instrumentIndex].Select(file => System.IO.Path.GetFileNameWithoutExtension(file)).ToArray();
            //E:\000 USERDB\cctv\MIDI\WpfApp2Wavmidi\bin\Debug\soundset\cello\cello - as2.wav

            scalesettingTXT.Text = "";

            foreach (string notes in noteValues)
            {
                if (notes != "")
                {
                    string[] notesTXT = notes.Split('=');
                    //notesTXT[0]=도 notesTXT[1]=C3 notesTXT[2]=1,33
                    //wavFiles.FindIndex(file => file == "C3")
                    if (notesTXT[1] != "")
                    {
                        //int damaged = Array.FindIndex(lines, l => l.Contains(kDamaged));
                        int index = Array.FindIndex(wavfile, l => l.Contains(notesTXT[1].ToLower()));
                        if (index != -1)
                        {
                            notesTXT[2] = $"{instrumentIndex},{index}";
                        }
                        scalesettingtxt.AppendText(index.ToString());
                    }

                    scalesettingTXT.AppendText($"{notesTXT[0]}={notesTXT[1]}={notesTXT[2]}_");
                }




            }




            ScaleSettingProcess();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            registGlobalKey(false);
        }

        private void datagridscaleSetting_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            // 인덱스를 텍스트박스에 출력 (몇번째 폴더의 몇번째 파일)
            if (midindextxt.Text == null) { return; }

            if (datagridscaleSetting.SelectedCells.Count > 0)
            {
                // 첫 번째 선택된 셀을 가져옴
                DataGridCellInfo selectedCell = datagridscaleSetting.SelectedCells[0];

                // 셀이 속한 행의 데이터를 가져옴
                var rowData = selectedCell.Item as IntervalData;

                // 선택된 셀의 열 인덱스 구하기
                int columnIndex = datagridscaleSetting.Columns.IndexOf(selectedCell.Column);

                // 셀의 값을 변경
                if (rowData != null)
                {
                    if (columnIndex == 2) // 이름 열
                    {
                        rowData.Values[2] = midindextxt.Text; // 원하는 값으로 변경

                        datagridscaleSetting.Items.Refresh();
                    }

                }
            }
        }
        private int NumRadioButtons = 12;
        private int currentRadioIndex = 0;
        private System.Timers.Timer timer;
        //private Stopwatch stopwatch = new Stopwatch();
        private TimeSpan lastElapsedTime;
        private int tick = 100;
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            metronomePanel();

            // Initialize and start the timer
            timer = new System.Timers.Timer
            {
                Interval = TimeSpan.FromMilliseconds(30).TotalMilliseconds//TimeSpan.FromSeconds( int.Parse(playintervaltxt.Text)) // Default interval
            };
            timer.Elapsed += Timer_Tick;

            /*
            // Initialize and start the precise timer
            timer = new Timer(100); // Interval in milliseconds, adjust as needed
            timer.Elapsed += Tick;
            timer.Start();
            */
            folderIndex5 = int.Parse(midindextxt.Text.Split(',')[0]);
            fileIndex5 = int.Parse(midindextxt.Text.Split(',')[1]);
            playtimefloat = float.Parse(playtimetxt.Text);

            MetronomeStartBtn.IsEnabled = true;
        }

        private void metronomePanel()
        {
            stackpanel1.Children.Clear();
            /*
            // Create a Slider
            Slider slider = new Slider
            {
                Minimum = 1,
                Maximum = NumRadioButtons,
                TickFrequency = 1,
                IsSnapToTickEnabled = true,
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };
            slider.ValueChanged += Slider_ValueChanged;
            stackpanel1.Children.Add(slider);
            */
            // Create RadioButtons
            for (int i = 0; i < NumRadioButtons; i++)
            {
                System.Windows.Controls.RadioButton radioButton = new System.Windows.Controls.RadioButton
                {
                    Content = $"{i}",// $"Radio {i}",
                    GroupName = "group",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                    //Style = (Style)FindResource("DefaultRadioButtonStyle")
                };

                if (i == 0) // Apply custom style to the first radio button
                {
                    //radioButton.Style = (Style)FindResource("RedDotRadioButtonStyle");
                }

                stackpanel1.Children.Add(radioButton);
            }
        }
        /*
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            timer.Interval = TimeSpan.FromSeconds(e.NewValue); // Update timer interval based on slider value
        }*/
        int folderIndex5 = 0;
        int fileIndex5 = 0;
        int folderIndex6 = 0;
        int fileIndex6 = 0;
        float playtimefloat = 0.0f;
        private void Timer_Tick(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DateTime now = DateTime.Now; //int milliseconds = now.Millisecond;

                TimeSpan currentTime = now.TimeOfDay;// stopwatch.Elapsed;
                TimeSpan difftime = currentTime - lastElapsedTime;
                if (difftime >= TimeSpan.FromMilliseconds(tick))  //|| difftime >= TimeSpan.FromMilliseconds(tick-tick*.3)
                {
                    elapsedtimeTxt.Text = difftime.ToString();
                    BPMTXT.Text = (60000 / difftime.Milliseconds).ToString();

                    lastElapsedTime = currentTime;
                    if (currentRadioIndex + 1 > NumRadioButtons)
                    {
                        currentRadioIndex = 0;
                    }

                    System.Windows.Controls.RadioButton radioButton = stackpanel1.Children[currentRadioIndex] as System.Windows.Controls.RadioButton; // +1 to skip slider
                    radioButton.IsChecked = true;
                    //PlaySound(currentRadioIndex);
                    // 인덱스를 텍스트박스에 출력 (몇번째 폴더의 몇번째 파일)
                    metronomeTxt3.Text = currentRadioIndex.ToString();
                    if (MetronomeMuteCheck.IsChecked == false)
                    {
                        if (currentRadioIndex > 0)
                        {
                            Task.Run(() => PlayWav(folderIndex5, fileIndex5, playtimefloat));
                        }
                        else
                        {
                            Task.Run(() => PlayWav(folderIndex6, fileIndex6, playtimefloat));
                        }
                    }

                    currentRadioIndex++;
                }
            });
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //timer.Start();
            if (timer != null)
            {
                if (!timer.Enabled)
                {
                    MetronomeStartBtn.Content = "Stop";
                    folderIndex5 = int.Parse(Metronometxt1.Text.Split(',')[0]);
                    fileIndex5 = int.Parse(Metronometxt1.Text.Split(',')[1]);
                    folderIndex6 = int.Parse(Metronometxt2.Text.Split(',')[0]);
                    fileIndex6 = int.Parse(Metronometxt2.Text.Split(',')[1]);
                    playtimefloat = float.Parse(playtimetxt.Text);
                    timer.Start();
                    //stopwatch.Start();
                }
                else
                {
                    MetronomeStartBtn.Content = "Start";
                    folderIndex5 = int.Parse(Metronometxt1.Text.Split(',')[0]);
                    fileIndex5 = int.Parse(Metronometxt1.Text.Split(',')[1]);
                    folderIndex6 = int.Parse(Metronometxt2.Text.Split(',')[0]);
                    fileIndex6 = int.Parse(Metronometxt2.Text.Split(',')[1]);
                    playtimefloat = float.Parse(playtimetxt.Text);
                    timer.Stop();
                    //stopwatch.Stop();
                }
            }

        }

        private void playintervaltxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            tick = int.Parse(playintervaltxt.Text);
            /*
            if (timer != null)
            {
                timer.Interval = TimeSpan.FromMilliseconds(int.Parse(playintervaltxt.Text)); // Default interval
            }
            */

        }

        private void Metronometxt1_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Metronometxt1.Text = midindextxt.Text;

        }

        private void Metronometxt2_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Metronometxt2.Text = midindextxt.Text;

        }

        private void MetronomeBitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (MetronomeBitSlider != null)
            {
                NumRadioButtons = (int)MetronomeBitSlider.Value;
                metronomePanel();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            functionstart1();
        }

        private void functionstart1()
        {
            ExcelClear();
            //섬집아기
            ScaleTxt.Text = "도,파,솔,라,솔,파,솔x3,솔x2,쉼표,라,파,솔,파,레,도x3,도x2,쉼표,\r\n도,파,솔,라,솔,파,솔x3,솔x2,쉼표,라x2,파,솔,레,미,파x3,파x2,쉼표,\r\n솔,솔,솔,파,솔,라,파x3,파x2,쉼표,^레x2,^레,^도x2,라,솔x3,솔x2,쉼표,\r\n^도,라,솔,파,솔,라,레x3,레x2,쉼표,도x2,파,미,파,솔,파x3,파x2,쉼표";
            scalesettingTXT.Text = "^도=c4=1,34_^레=d4=1,42_도=c3=1,33_레=d3=1,41_미=e3=1,55_파=f3=1,69_솔=g3=1,76_라=a3=1,3_시=b3=1,18_";
            playintervalslider.Value = 220;
            playtimeslider.Value = 4;

            autoprepareloadmusic();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ExcelClear();
            //베토벤바이러스쉬운악보
            ScaleTxt.Text = "^미,^라,^시,^^도x3,^^레,^시x3,^^도,^라x6,^^도,^^레,^^미x2,^^미x2,^^미x2,^^미x2,\r\n^^미x6,^^레,^^미,^^파x4,^시x2,^^도,^^레,^^미x4,^라x2,^라,^시,^^도x2,^^도,^^레,^시x2,^시,^^도,\r\n^라x4,쉼표,^미,^라,^시,^^도x3,^^레,^시x3,^^도,^라x6,^^도,^^레,^^미x2,^^미x2,^^미x2,^^미x2,\r\n^^미x6,^^레,^^미,^^파x4,^시x2,^^도,^^레,^^미x4,^라x2,^라,^시,^^도x2,^^도,^^레,^시x2,^시,^도,\r\n^라x8,^시x2,^미x2,^^미x2,^^레x2,^^레x2,^^도x2,^시x2,^라x2,^시x2,^솔x2,^^솔x2,^^파x2,\r\n^^파x2,^^미x2,^^레x2,^^도x2,^라x4,^^라x2,^^솔x2,^^솔x2,^^파x2,^^미x2,^^레x2,^시x4,^^시x2,^^라x2,\r\n^^라x4,^^미x4,쉼표x5,^미,^라,^시,^^도x3,^^레,^시x3,^^도,^라x6,^^도,^^레,\r\n^^미x2,^^미x2,^^미x2,^^미x2,^^미x6,^^레,^^미,^^파x4,^시x2,^^도,^^레,\r\n^^미x4,^라x2,^라,^시,^^도x2,^^도,^^레,^시x2,^시,^^도,^라x8,^라x8\r\n+\r\n쉼표x3,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,\r\n라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,\r\n라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,\r\n라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,\r\n라x2,라x2,라x2,라x2,미x2,미x2,미x2,미x2,라x2,라x2,라x2,라x2,솔x2,솔x2,솔x2,솔x2,\r\n도x2,도x2,도x2,도x2,라x2,라x2,라x2,라x2,레x2,레x2,레x2,레x2,시x2,시x2,시x2,시x2,\r\n미x2,미x2,미x2,미x2,미x2,미x2,미x4,라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,\r\n라x2,라x2,라x2,라x2,라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,\r\n라x2,라x2,라x2,라x2,레x2,레x2,미x2,미x2,라x2,라x2,라x2,라x2,라x8";
            scalesettingTXT.Text = "^^b미=eb5=1,64_^#파=GB4=1,84_^#솔=AB4=1,11_#솔=AB3=1,10_^^도=C5=1,35_^^레=D5=1,43_^^미=E5=1,57_^^파=F5=1,71_^^솔=G5=1,78_^^라=A5=1,5_^^시=B5=1,20_^도=C4=1,34_^레=D4=1,42_^미=E4=1,56_^파=F4=1,70_^솔=G4=1,77_^라=A4=1,4_^시=B4=1,19_도=C3=1,33_레=D3=1,41_미=E3=1,55_파=F3=1,69_솔=G3=1,76_라=A3=1,3_시=B3=1,18_";
            playintervalslider.Value = 250;
            playtimeslider.Value = 2;

            autoprepareloadmusic();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ExcelClear();
            //베토벤 월광
            ScaleTxt.Text = "쉼표,Ab1,Db2,E2,Ab2,Db2,E2,Ab2,Db3,E3,Ab2,Db3,E3,Ab2,Db3,E3,Ab3,Db4,E3,Ab3,Db4,E4,Ab3,Db4,E4,Ab3,Db4,E4,Ab4x2&E4x2&Db4x2&Ab3x2,Ab4x2&E4x2&Db4x2&Ab3x2,\r\n쉼표,Ab1,C2,Eb2,Ab2,C2,Eb2,Ab2,C3,Eb2,Ab2,C3,Eb3,Ab2,C3,Eb3,Ab3,C4,Eb3,Ab3,C4,Eb4,Ab3,C4,Eb4,Ab3,C4,Eb4,Ab4x2&Eb4x2&C4x2&Ab3x2,Ab4x2&Eb4x2&C4x2&Ab3x2,\r\n+\r\nDb1x2,Ab2x2,Db1x2,Ab2x2,Db1x2,Ab2x2,Db1x2,Ab2x2,Db1x2,Ab2x2,Db1x2,Ab2x2,Db1x2,Ab2x2,Db1x2&Db2x2,Ab2x2,\r\nC1x2,Ab2x2,C1x2,Ab2x2,C1x2,Ab2x2,C1x2,Ab2x2,C1x2,Ab2x2,C1x2,Ab2x2,C1x2,Ab2x2,C1x2&C2x2,Ab2x2";
            scalesettingTXT.Text = "Db1=Db1=1,46_Ab1=Ab1=1,8_C1=C1=1,31_D1=D1=1,39_E1=E1=1,53_F1=F1=1,67_G1=G1=1,74_A1=A1=1,1_B1=B1=1,16_Eb2=Eb2=1,61_Db2=Db2=1,47_Ab2=Ab2=1,9_C2=C2=1,32_D2=D2=1,40_E2=E2=1,54_F2=F2=1,68_G2=G2=1,75_A2=A2=1,2_B2=B2=1,17_Eb3=Eb3=1,62_Db3=Db3=1,48_Ab3=Ab3=1,10_C3=C3=1,33_D3=D3=1,41_E3=E3=1,55_F3=F3=1,69_G3=G3=1,76_A3=A3=1,3_B3=B3=1,18_Eb4=Eb4=1,63_Db4=Db4=1,49_Ab4=Ab4=1,11_C4=C4=1,34_D4=D4=1,42_E4=E4=1,56_F4=F4=1,70_G4=G4=1,77_A4=A4=1,4_B4=B4=1,19_";
            playintervalslider.Value = 500;
            playtimeslider.Value = 2;


            autoprepareloadmusic();
        }

        private async void autoprepareloadmusic()
        {
            ConvertorBTN.IsEnabled = true;
            PrepareMusicBTN.IsEnabled = true;

            ScaleSettingProcess();
            ScaleTxttoColumnBTN.IsEnabled = true;

            initializeingGrid();

            GridAddColumn();
            ScaleTxttoColumnBTN_Click_Function();

            preparemusicbtn_Function();

            LoadFilesBTN.IsEnabled = false;

            //prepareMusicNotes();
            //PlayBTN.IsEnabled = true;
            await Task.Run(() => prepareMusicNotes());

            //playSheet();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //registGlobalKey(false);//true : regist false : unregist
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            //registGlobalKey(true); //true : regist false : unregist
        }


        private static string GetPrefix(string part) { var equalsIndex = part.IndexOf('='); return equalsIndex > 0 ? part.Substring(0, equalsIndex) : part; }
        private static string GetSuffix(string part) { var equalsIndex = part.LastIndexOf('='); return equalsIndex > 0 ? part.Substring(equalsIndex) : part; }

        private async Task autopargescalesettingbyfilename()
        {
            if (InstrumentTxt.Text.Split(',').Count() == 2)
            {
                //scalesettingTXT.Text = InstrumentTxt.Text;

                // 텍스트 박스에서 헤더를 가져오기
                int folderindex7 = int.Parse(InstrumentTxt.Text.Split(',')[0].Trim()); // 필요한 인덱스를 사용하고 공백 제거
                                                                                       //scalesettingTXT.AppendText($"__{folderindex7.ToString()}___");

                if (folderindex7 >= 0 && folderindex7 < wavFiles.Count)
                {
                    scalesettingTXT.Text = "";
                    Regex regex = new Regex("[a-zA-Z]{1,2}\\d+");
                    int fileindexed = 0;
                    List<string> extractedValues = new List<string>();

                    foreach (var ff in wavFiles[folderindex7])
                    { // Your logic here, e.g. processing each file }
                        string currentFileName = System.IO.Path.GetFileName(ff);

                        Match match = regex.Match(ff);
                        if (match.Success)
                        {
                            //Db1=Db1=1,46_
                            string temp = $"{match.Value}={match.Value}={folderindex7},{fileindexed}_";
                            extractedValues.Add(temp);
                            //scalesettingTXT.AppendText(temp);
                        }
                        fileindexed++;

                    }
                    //var sortedValues = extractedValues.OrderBy(x => x).ToList();
                    //var sortedValues = extractedValues.OrderByDescending(x => x).ToList();
                    var sortedValues = extractedValues.OrderByDescending(p => GetPrefix(p).Length).ThenBy(p => GetPrefix(p)).ThenBy(p => GetSuffix(p)).ToList();
                    //OrderByDescending parts.OrderByDescending(p => GetPrefix(p).Length) .ThenBy(p => GetPrefix(p)) .ThenBy(p => GetSuffix(p))
                    foreach (var value in sortedValues)
                    {
                        scalesettingTXT.AppendText(value);
                        //Console.WriteLine(value); // Outputs: a0, a1, a2, a3, ab1, ab2 }
                    }
                }
                else
                {
                    Console.WriteLine("Invalid folder index.");
                }
            }
        }
        private async void LoadSettingFileNameBtn_Click(object sender, RoutedEventArgs e)
        {
            await autopargescalesettingbyfilename();
            ConvertorBTN.IsEnabled = true;
            PrepareMusicBTN.IsEnabled = true;
            RecordBtn.IsEnabled = true;

            ScaleSettingProcess();
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            if (sequenceshowtogglebtn.Content == "Sequence Show")
            {
                sequenceshowtogglebtn.Content = "Sequence Hide";
                SetLabelVisibility(this);
            }
            else
            {
                sequenceshowtogglebtn.Content = "Sequence Show";
                SetLabelVisibility(this);
            }
        }

        private void SetLabelVisibility(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is System.Windows.Controls.Label label && label.Name.StartsWith("sequence"))
                {
                    if (label.Visibility == Visibility.Visible)
                    {
                        label.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        label.Visibility = Visibility.Visible;
                    }
                    //if (sequencelabels.IndexOf(label) < 0) { sequencelabels.Add(label); }
                }
                else
                {
                    SetLabelVisibility(child); // 재귀적으로 자식 요소 탐색
                }
            }
        }

        private void sequencelabelprepare(DependencyObject parent)
        {

            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is System.Windows.Controls.Label label && label.Name.StartsWith("sequence"))
                {
                    if (sequencelabels.IndexOf(label) < 0)
                    {
                        sequencelabels.Add(label);
                        //scalesettingtxt.AppendText(label.Name+"\r\n"); 
                    }
                }
                else
                {
                    sequencelabelprepare(child); // 재귀적으로 자식 요소 탐색
                }
            }
        }

        private List<System.Windows.Controls.Label> sequencelabels = new List<System.Windows.Controls.Label>();
        private Line currentArrow;
        private Polygon currentArrowHead;

        private void AddArrowHead(Line line)
        {
            // 화살표 끝점에 작은 삼각형 그리기
            const double arrowHeadSize = 10;
            var angle = Math.Atan2(line.Y2 - line.Y1, line.X2 - line.X1) * 180 / Math.PI;

            currentArrowHead = new Polygon
            {
                Fill = Brushes.Red,
                Points = new PointCollection
        {
            new Point(line.X2, line.Y2),
            new Point(line.X2 - arrowHeadSize, line.Y2 - arrowHeadSize / 2),
            new Point(line.X2 - arrowHeadSize, line.Y2 + arrowHeadSize / 2)
        },
                RenderTransform = new RotateTransform(angle, line.X2, line.Y2)
            };

            arrowCanvas.Children.Add(currentArrowHead);
        }


        private void Label_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var currentLabel = sender as System.Windows.Controls.Label;
            int index = int.Parse(currentLabel.Name.Substring("sequence".Length));//sequencelabels.FindIndex(label => label.Name == currentLabel.Name); //sequencelabels.IndexOf(currentLabel);
            //index = sequencelabels.IndexOf(currentLabel);
            // 현재 레이블이 리스트의 마지막 레이블이 아닌 경우에만 다음 레이블로 화살표를 그립니다.
            if (index != -1 && index < sequencelabels.Count - 1)
            {
                var nextLabel = sequencelabels[index];
                DrawArrow(currentLabel, nextLabel);
                scalesettingtxt.AppendText($"{currentLabel.Name},{nextLabel.Name},{index} \r\n");
                scalesettingtxt.ScrollToEnd();
            }
        }

        private void Label_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // 마우스가 떠나면 화살표를 제거합니다.
            if (currentArrow != null)
            {
                arrowCanvas.Children.Remove(currentArrow);
                currentArrow = null;
                arrowCanvas.Children.Remove(currentArrowHead);
                currentArrowHead = null;
            }
        }

        private void DrawArrow(System.Windows.Controls.Label startLabel, System.Windows.Controls.Label endLabel)
        {
            // 레이블의 중심점 계산
            var startPoint = new Point(startLabel.Margin.Left + startLabel.ActualWidth / 2, startLabel.Margin.Top + startLabel.ActualHeight / 2);
            var endPoint = new Point(endLabel.Margin.Left + endLabel.ActualWidth / 2, endLabel.Margin.Top + endLabel.ActualHeight / 2);

            if (Canvas.GetLeft(startLabel) > 0)
            {
                startPoint = new Point(Canvas.GetLeft(startLabel) + startLabel.ActualWidth / 2, Canvas.GetTop(startLabel) + startLabel.ActualHeight / 2);

            }

            if (Canvas.GetLeft(endLabel) > 0)
            {
                endPoint = new Point(Canvas.GetLeft(endLabel) + endLabel.ActualWidth / 2, Canvas.GetTop(endLabel) + endLabel.ActualHeight / 2);
            }



            // 화살표 선을 생성
            currentArrow = new Line
            {
                Stroke = Brushes.Red,
                StrokeThickness = 2,
                X1 = startPoint.X,
                Y1 = startPoint.Y,
                X2 = endPoint.X,
                Y2 = endPoint.Y
            };

            // Canvas에 선 추가
            arrowCanvas.Children.Add(currentArrow);

            AddArrowHead(currentArrow);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 텍스트박스를 배열로 관리
            fileTextBoxes = new System.Windows.Controls.TextBox[] { txtFile1, txtFile2, txtFile3, txtFile4, txtFile5, txtFile6, txtFile7, txtFile8, txtFile9 };


            datagridview1.AutoGenerateColumns = false; // 자동 생성 방지

            folderload();
            VolumeCtrl.Value = 100;
            volume = 1.00f;

            foreach (System.Windows.Controls.TextBox temp in fileTextBoxes)
            {
                temp.PreviewMouseDown += new System.Windows.Input.MouseButtonEventHandler(NumEventHandler);
            }

            sequencelabelprepare(this);

            foreach (var label in sequencelabels)
            {
                label.MouseEnter += Label_MouseEnter;
                label.MouseLeave += Label_MouseLeave;
            }

            foreach (var label in sequencelabels)
            {
                scalesettingtxt.AppendText(label.Name + "\r\n");
            }

            simplesetting();
        }

        private void RecordBtn_Click(object sender, RoutedEventArgs e)
        {
            if (RecordBtn.Content.ToString().Contains("● Rec"))
            {
                RecordBtn.Content = "■ Stop";
                makescaletxtbtn.IsEnabled = false;
            }
            else
            {
                RecordBtn.Content = "● Rec";
                makescaletxtbtn.IsEnabled = true;
                intervalchangeBTN.IsEnabled = true;
            }

        }

        //private bool selectedon = false;

        private void RecordProcess(string midiindex)
        {
            scalesettingtxt.AppendText(midiindex + "\r\n");
            scalesettingtxt.ScrollToEnd();
            if (RecordBtn.Content.ToString().Contains("● Rec"))
            {
                return;
            }

            PrepareMusicBTN.IsEnabled = true;
            ClearExcel.IsEnabled = true;

            int columnIndex;
            bool isValidIndex = int.TryParse(IndexofCollumn.Text, out columnIndex); // 문자열을 정수로 변환 시도
            // 텍스트 박스에서 값 가져오기
            int interval = 0;
            bool intervaltxtparsesuccess = int.TryParse(playintervaltxt.Text, out interval);
            List<string> noteValues = new List<string>();// { $"{midiindex}" };
            noteValues.Add(midiindex);

            if (Intervalsetting == null)
            {
                System.Windows.MessageBox.Show("Need scale settings~~~~ ");
                return;
            }

            foreach (var item in Intervalsetting)
            {
                if (item.Values.Count > 2)
                {
                    if (midiindex == item.Values[2])
                    {
                        midiindex = item.Values[0];
                    }
                }
            }


            if (Intervals == null)
            {
                initializeingGrid();
                GridAddColumn();
            }
            int beforeItemscount = Intervals.Count();
            if (datagridview1.SelectedCells.Count > 0)//|| selectedon)
            {
                //selectedon = true;
                ModScaleRecFunction(midiindex);
            }
            else
            {
                Intervals.Add(new IntervalData { Values = new List<string> { (beforeItemscount).ToString(), interval.ToString(), midiindex } });
            }




        }

        private void ModScaleRecFunction(string midiindex)
        {
            if (midiindex == "null")
            {
                midiindex = ScaleTxt.Text;
            }

            int rows = int.Parse(SelectedInstrumentRowTXT.Text);
            int column = int.Parse(SelectedInstrumentColumnTXT.Text);
            int i = 0;
            SelectedInstrumentRowTXT.Text = (rows + 1).ToString();
            if (rows + 1 > Intervals.Count)
            {
                ModAddindex(1, rows, column);
            }
            //datagridview1.Items.Refresh();


            //foreach (string note in noteValues)
            foreach (var item in Intervals)
            {
                if (i >= rows && i < (1 + rows))
                {
                    item.Values[column] = midiindex;// "item.Values[" +column.ToString()+","+ (i - rows).ToString() + "]";// 
                }
                i++;
            }
            datagridview1.Items.Refresh();

            datagridview1.SelectedCells.Add(new DataGridCellInfo(datagridview1.Items[rows + 1], datagridview1.Columns[column]));
        }

        private void focusoutbtn_Click(object sender, RoutedEventArgs e)
        {

        }


        private void makescaletxtbtn_Click(object sender, RoutedEventArgs e)
        {
            LoadFilesBTN.IsEnabled = true;

            //            List<string> temps = new List<string>();
            string temps = "";
            int beforevalueindex = 0;

            int values = datagridview1.Columns.Count();
            for (int i = 2; i < values; i++)
            {
                foreach (var item in Intervals)
                {
                    if (item.Values[i] != "")
                    {
                        temps += item.Values[i].Replace("0,0", "쉼표") + ",";
                    }
                }
                if (i != values) { temps += "+"; }
            }

            // `ScaleTxt`의 내용을 초기화하고, `temps` 리스트의 내용을 추가
            ScaleTxt.Text = temps;
        }

        private void addrowbtn_Click(object sender, RoutedEventArgs e)
        {
            RecordProcess("쉼표");
        }

        private Color rowrgb;

        private void datagridview1_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            Random random = new Random();

            if (rowrgb == null)
            {
                byte r = (byte)(random.Next(128) + 128);
                byte g = (byte)(random.Next(128) + 128);
                byte b = (byte)(random.Next(128) + 128);
                rowrgb = Color.FromRgb(r, g, b);
            }
            // 행의 인덱스가 4로 나누어 떨어질 때마다 배경색을 랜덤으로 설정
            if ((e.Row.GetIndex() + 1) % 4 == 0)
            {
                // 랜덤 색상 생성
                byte r = (byte)(random.Next(128) + 128);
                byte g = (byte)(random.Next(128) + 128);
                byte b = (byte)(random.Next(128) + 128);
                rowrgb = Color.FromRgb(r, g, b);
                e.Row.Background = new SolidColorBrush(rowrgb);
            }
            else
            {
                // 나머지 행은 기본 색상으로 설정
                e.Row.Background = new SolidColorBrush(rowrgb);
            }
        }

        private async void Button_Click_9(object sender, RoutedEventArgs e)
        {
            ExcelClear();

            await autopargescalesettingbyfilename();
            ConvertorBTN.IsEnabled = true;
            PrepareMusicBTN.IsEnabled = true;
            RecordBtn.IsEnabled = true;

            ScaleSettingProcess();

            //베토벤 월광
            ScaleTxt.Text = "e6,eb6,d6,db6,d6,db6,c6,b5,c6,b5,bb5,a5,ab5,g5,gb5,f5,e5,eb5,d5,db5,d5,db5,c5,b4,c5,b4,bb4,a4,ab4,g4,gb4,f4,eb4,d4,db4,d4,db4,c4,b3,e4,eb4,d4,db4,d4,db4,d4,b3,f4,eb4,d4,db4,d4,db4,c4,b3,e4,eb4,d4,db4,d4,db4,d4,b3,e4,eb4,d4,db4,c4,f4,e4,eb4,e4,eb4,d4,db4,c4,db4,d4,eb4";
            playintervalslider.Value = 600;
            playtimeslider.Value = 2;


            autoprepareloadmusic();
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            LoadFilesBTN.IsEnabled = true;

            //            List<string> temps = new List<string>();
            string temps = "";
            int beforevalueindex = 0;

            int values = datagridview1.Columns.Count();
            for (int i = 2; i < values; i++)
            {
                foreach (var item in Intervals)
                {
                    if (item.Values[i] != "")
                    {
                        temps += item.Values[i].Replace("0,0", "쉼표") + ",";
                    }
                }
                if (i != values) { temps += "+"; }
            }

            // `ScaleTxt`의 내용을 초기화하고, `temps` 리스트의 내용을 추가
            ScaleTxt.Text = temps;

            foreach (var item in Intervalsetting)
            {
                if (item.Values.Count > 2)
                {
                    string stringToRemove = item.Values[0].ToLower();
                    ScaleTxt.Text = ScaleTxt.Text.ToLower();
                    ScaleTxt.Text = ScaleTxt.Text.Replace(stringToRemove, item.Values[1]);
                }
            }

            //ScaleTxt.Text = ScaleTxt.Text.Replace("0,0","쉼표");

        }

    }
}

