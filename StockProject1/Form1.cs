using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StockProject1
{
    public partial class Form1 : Form
    {
        List<ItemInfo> itemInfoList;
        List<ItemPriceEntity> itemPriceEntityList;

        Series candleSeries;

        public Form1()
        {
            InitializeComponent();

            loginButton.Click += ButtonClicked;
            loginStateButton.Click += ButtonClicked;
            searchButton.Click += ButtonClicked;

            axKHOpenAPI1.OnEventConnect += OnEventConnect;
            axKHOpenAPI1.OnReceiveTrData += OnReceiveTrData;

            candleSeries = chart.Series["Series1"];
            candleSeries["PriceUpColor"] = "Red";
            candleSeries["PriceDownColor"] = "Blue";
        }

        void requestUserInfo()
        {
            string name = axKHOpenAPI1.GetLoginInfo("USER_NAME");
            nameLabel.Text = name;

            string id = axKHOpenAPI1.GetLoginInfo("USER_ID");
            idLabel.Text = id;

            string accountCnt = axKHOpenAPI1.GetLoginInfo("ACCOUNT_CNT");
            accountCntLabel.Text = accountCnt;

            string accountList = axKHOpenAPI1.GetLoginInfo("ACCLIST");
            accountListLabel.Text = accountList;

        }

        void requestItemCodeList()
        {
            itemInfoList = new List<ItemInfo>();

            string codeList = axKHOpenAPI1.GetCodeListByMarket("0");
            string[] codeArray = codeList.Split(';');

            for (int i = 0; i < codeArray.Length; i++)
            {
                string itemName = axKHOpenAPI1.GetMasterCodeName(codeArray[i]);
                ItemInfo item = new ItemInfo(codeArray[i], itemName);
                itemInfoList.Add(item);
                
            }
        }


        void OnReceiveTrData(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnReceiveTrDataEvent e)
        {
            if (e.sRQName == "일별주가")
            {
                int count = axKHOpenAPI1.GetRepeatCnt(e.sTrCode, e.sRQName);

                candleSeries.Points.Clear(); // 다른 종목 검색시 캔들이 누적되는 것을 방지하기 위해

                // Y축 값 스케일링을 위해서 저가, 고가 값을 저장
                int minValue = int.MaxValue;
                int maxValue = int.MinValue;

                for (int i = 0; i < count; i++)
                {
                    string 날짜 = axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "날짜").Trim();
                    int 시가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "시가"));
                    int 고가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "고가"));
                    int 저가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "저가"));
                    int 종가 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "종가"));
                    int 전일비 = int.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "전일비"));
                    double 등락률 = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "등락률"));
                    long 거래량 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "거래량"));
                    long 금액 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "금액(백만)"));
                    double 신용비 = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "신용비"));
                    long 개인 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "개인").Replace("--","-"));
                    long 기관 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "기관").Replace("--", "-"));
                    long 외인수량 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "외인수량").Replace("--", "-"));
                    long 외국계 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "외국계").Replace("--", "-"));
                    long 프로그램 = long.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "프로그램").Replace("--", "-"));
                    double 외인비 = double.Parse(axKHOpenAPI1.GetCommData(e.sTrCode, e.sRQName, i, "외인비").Replace("--", "-"));

                    if (시가 < 0) 시가 = -시가;
                    if (고가 < 0) 고가 = -고가;
                    if (저가 < 0) 저가 = -저가;
                    if (종가 < 0) 종가 = -종가;

                    //Console.WriteLine("날짜: " + 날짜 + "  종가: " + 종가);
                    itemPriceEntityList.Add(new ItemPriceEntity(날짜, 시가, 고가, 저가, 종가, 전일비, 등락률, 거래량, 금액, 신용비, 개인, 기관, 외인수량, 외국계, 프로그램, 외인비));

                    if (minValue > 저가)
                        minValue = 저가;
                    if (maxValue < 고가)
                        maxValue = 고가;

                    candleSeries.Points.AddXY(날짜, 고가);
                    candleSeries.Points[i].YValues[1] = 저가;
                    candleSeries.Points[i].YValues[2] = 시가;
                    candleSeries.Points[i].YValues[3] = 종가;

                    candleSeries.Points[i].ToolTip = "일자: " + 날짜 + "\n"
                        + "시가: " + String.Format("{0:#,###}", 시가) + "\n"
                        + "종가: " + String.Format("{0:#,###}", 종가) + "\n"
                        + "고가: " + String.Format("{0:#,###}", 고가) + "\n"
                        + "저가: " + String.Format("{0:#,###}", 저가) + "\n"
                        + "거래량: " + String.Format("{0:#,###}", 거래량);

                }

                dataGridView.DataSource = itemPriceEntityList;

                // 차트 스케일링
                chart.ChartAreas["ChartArea1"].AxisY.Maximum = maxValue;
                chart.ChartAreas["ChartArea1"].AxisY.Minimum = minValue;
            }
        }

        void OnEventConnect(object sender, AxKHOpenAPILib._DKHOpenAPIEvents_OnEventConnectEvent e)
        {
            if(e.nErrCode == 0)
            {
                //MessageBox.Show("정상적으로 로그인 되었습니다");
                requestUserInfo();
                requestItemCodeList();

                // 검색 소스에 종목명 넣기
                AutoCompleteStringCollection source = new AutoCompleteStringCollection();
                for (int i = 0; i < itemInfoList.Count; i++)
                {
                    source.Add(itemInfoList[i].itemName);
                }
                searchTextBox.AutoCompleteCustomSource = source;

            }
            else if (e.nErrCode == 100)
            {
                MessageBox.Show("사용자 정보교환 실패");
            }
            else if (e.nErrCode == 101)
            {
                MessageBox.Show("서버접속 실패");
            }
            else if (e.nErrCode == 102)
            {
                MessageBox.Show("버전처리 실패");
            }
            else
            {
                MessageBox.Show("알 수 없는 오류 발생");
            }
        }


        private void ButtonClicked(object sender, EventArgs e)
        {
            if (sender.Equals(loginButton))
            {
                axKHOpenAPI1.CommConnect();
            }
            else if (sender.Equals(loginStateButton))
            {
                int res = axKHOpenAPI1.GetConnectState();
                if (res == 0)
                {
                    MessageBox.Show("로그인 되지 않았습니다.");
                }
                else if (res == 1)
                {
                    MessageBox.Show("로그인 되었습니다.");
                }
            }
            else if (sender.Equals(searchButton))
            {
                string itemName = searchTextBox.Text;
                DateTime dt = dateTimePicker.Value;
                String date = dt.ToString("yyyyMMdd");

                for (int i = 0; i < itemInfoList.Count; i++)
                {
                    if (itemName == itemInfoList[i].itemName)
                    {
                        itemPriceEntityList = new List<ItemPriceEntity>();

                        //MessageBox.Show(itemInfoList[i].itemCode);
                        axKHOpenAPI1.SetInputValue("종목코드", itemInfoList[i].itemCode);
                        axKHOpenAPI1.SetInputValue("조회일자", date);
                        axKHOpenAPI1.SetInputValue("표시구분", "1");

                        axKHOpenAPI1.CommRqData("일별주가", "opt10086", 0, "5001");

                        break;
                    }
                }
            }

        }

    }

    class ItemInfo
    {
        public string itemCode;
        public string itemName;

        public ItemInfo(string itemCode, string itemName)
        {
            this.itemCode = itemCode;
            this.itemName = itemName;
        }
    }

    class ItemPriceEntity
    {
        public string 날짜 { get; set; }
        public int 시가 { get; set; }
        public int 고가 { get; set; }
        public int 저가 { get; set; }
        public int 종가 { get; set; }
        public int 전일비 { get; set; }
        public double 등락률 { get; set; }
        public long 거래량 { get; set; }
        public long 금액 { get; set; }
        public double 신용비 { get; set; }
        public long 개인 { get; set; }
        public long 기관 { get; set; }
        public long 외인수량 { get; set; }
        public long 외국계 { get; set; }
        public long 프로그램 { get; set; }
        public double 외인비 { get; set; }

        public ItemPriceEntity() { }

        public ItemPriceEntity(string 날짜, int 시가, int 고가, int 저가, int 종가, int 전일비, double 등락률, long 거래량, long 금액, double 신용비, long 개인, long 기관, long 외인수량, long 외국계, long 프로그램, double 외인비)
        {
            this.날짜 = 날짜;
            this.시가 = 시가;
            this.고가 = 고가;
            this.저가 = 저가;
            this.종가 = 종가;
            this.전일비 = 전일비;
            this.등락률 = 등락률;
            this.거래량 = 거래량;
            this.신용비 = 신용비;
            this.개인 = 개인;
            this.기관 = 기관;
            this.외인수량 = 외인수량;
            this.외국계 = 외국계;
            this.프로그램 = 프로그램;
            this.외인수량 = 외인수량;
            this.외인비 = 외인비;
        }
    }
}
