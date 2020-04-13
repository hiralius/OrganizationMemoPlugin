using Grabacr07.KanColleWrapper;
using Livet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Reflection;

namespace OrganizationMemoPlugin
{
    public class OrganizationViewModel : ViewModel
    {
        #region プロパティ

        public Boolean IsDisplayFleetNull => DisplayFleet == null;

        private OrganizationFleet _DisplayFleet;
        public OrganizationFleet DisplayFleet
        {
            get
            {
                return _DisplayFleet;
            }
            set
            {
                if (value == null && ItemFleets.Count() != 0) return;
                _DisplayFleet = value;
                IgnoreChange = true;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDisplayFleetNull));
            }
        }

        // 選択リスト用
        private ObservableCollection<OrganizationFleet> _ItemFleets;
        public ObservableCollection<OrganizationFleet> ItemFleets
        {
            get
            {
                return _ItemFleets;
            }
            set
            {
                _ItemFleets = value;
                RaisePropertyChanged();
            }
        }

        public Action<int> DropCallback { get { return MoveFleet; } }

        #endregion

        private List<OrganizationShipInfo> Fleet2OrganizationShipInfo(int i) =>
            KanColleClient.Current.Homeport.Organization.Fleets[i].Ships.Select(
                ship => new OrganizationShipInfo()
                {
                    Id = ship.Info.Id,
                    SlotIds = ship.Slots.Select(slot => slot.Item.Info.Id).ToList(),
                    ExSlot = ship.ExSlot.Item.Info.Id
                }
            ).ToList();

        // 艦隊リスト実体
        private OrganizationFleets _OrganizationFleets;

        #region 適用ボタン関連

        private bool IgnoreChange = false;
        private bool _Changed = false;

        public void TextInputing()
        {
            IgnoreChange = false;
        }

        public bool Changed
        {
            get { return _Changed; }
            set
            {
                if (value == _Changed) return;
                _Changed = value;
                RaisePropertyChanged();
            }
        }


        public void TextChanged()
        {
            if (IgnoreChange) return;
            Changed = true;
        }

        public void Apply()
        {
            SaveFile();
            Changed = false;
        }

        #endregion

        #region 艦隊操作

        public void AddFleet(String flag)
        {
            // 連合艦隊かどうか
            bool isCombined = Convert.ToBoolean(flag);
            var newFleet = new OrganizationFleet()
            {
                FirstFleet = Fleet2OrganizationShipInfo(1),
                SecondFleet = isCombined ? Fleet2OrganizationShipInfo(2) : null,
                FirstFleetName = KanColleClient.Current.Homeport.Organization.Fleets[1].Name,
                SecondFleetName = isCombined ? KanColleClient.Current.Homeport.Organization.Fleets[2].Name : null,
                Time = DateTime.Now
            };

            _OrganizationFleets.Fleets.Add(newFleet);

            ItemFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);

            DisplayFleet = newFleet;

            Task.Run(() => {
                SaveFile();
            });
        }

        public void DeleteFleet()
        {
            if (ItemFleets.Count() == 0 || DisplayFleet == null) return;

            int index = ItemFleets
                .Select((y, i) => new { Fleet = y, Index = i })
                .Where(f => f.Fleet.Time.Equals(DisplayFleet.Time))
                .First().Index;

            OrganizationFleet fleet = ItemFleets
                .Where((d, i) => i == index - 1 || i == index + 1)
                .FirstOrDefault();

            var fleets = ItemFleets.ToList();

            fleets.RemoveAt(index);

            _OrganizationFleets.Fleets = fleets;

            ItemFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);

            DisplayFleet = fleet;

            Task.Run(
                () => {
                    SaveFile();
                });
        }

        private void MoveFleet(int index)
        {
            if (index < 0) return;

            int currentIndex = ItemFleets
                .Select((y, i) => new { Fleet = y, Index = i })
                .Where(f => f.Fleet.Time.Equals(DisplayFleet.Time))
                .First().Index;

            Console.WriteLine("index={0}, current={1}", currentIndex, index);

            _ItemFleets.Move(currentIndex, index);

            var fleets = _ItemFleets.ToList();
            _OrganizationFleets.Fleets = fleets;

            Task.Run(
                () => {
                    SaveFile();
                });

            Changed = false;
            RaisePropertyChanged();
        }

        public void ChangeFleetName(String first, String second)
        {
            if (first != null)
            {
                _DisplayFleet.FirstFleetName = first;
            }
            if (second != null)
            {
                _DisplayFleet.SecondFleetName = second;
            }

            SaveFile();

            RaisePropertyChanged();
            RaisePropertyChanged(nameof(IsDisplayFleetNull));
        }

        #endregion

        #region ファイル操作

        /// <summary>
        /// このプラグインがあるフォルダに
        /// \アセンブリ名.xaml
        /// を繋げたもの（デフォルトはこれ）
        /// </summary>
        private static string FileName => "OrganizationMemo.txt";
        private static string FilePath { get; } = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName,
             FileName);

        private XmlSerializer serializer = new XmlSerializer(typeof(OrganizationFleets));

        private void SaveFile()
        {
            try
            {              
                //書き込むファイルを開く（UTF-8 BOM無し）
                var sw = new StreamWriter(FilePath, false, new UTF8Encoding(false));
                //シリアル化し、XMLファイルに保存する
                serializer.Serialize(sw, _OrganizationFleets);
                //ファイルを閉じる
                sw.Close();
            }
            catch (Exception)
            { 
            }
        }

        private OrganizationFleets LoadFile()
        {
            var fleets = new OrganizationFleets();
            try
            {
                //読み込むファイルを開く（UTF-8 BOM無し）
                var sr = new StreamReader(FilePath, new UTF8Encoding(false), false);
                //デシリアライズし、メンバにセットする
                fleets = serializer.Deserialize(sr) as OrganizationFleets;
                //ファイルを閉じる
                sr.Close();
            }
            catch (Exception)
            {
            }

            return fleets;
        }

        #endregion

        private void Init()
        {
            _OrganizationFleets = LoadFile();

            ItemFleets = new ObservableCollection<OrganizationFleet>(_OrganizationFleets.Fleets);
            if (_ItemFleets.Count > 0)
            {
                _DisplayFleet = _ItemFleets.First();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(IsDisplayFleetNull));
            }
        }

        public OrganizationViewModel()
        {
            Init();
        }

    }
}
