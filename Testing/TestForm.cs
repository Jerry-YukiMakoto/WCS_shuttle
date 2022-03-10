using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WCS_API_Server;
using Unity;
using Mirle.DB.Object;
using Mirle.ASRS.WCS.Library;


namespace test
{
    public partial class TestForm : Form
    {
        private WebApiHost _webApiHost;
        private UnityContainer _unityContainer;
        public TestForm()
        {
            InitializeComponent();
            cboFun.Items.Clear();
            cboFun.Items.Add("MOVE_TASK_ADD");
            cboFun.Items.Add("MOVE_TASK_FORCE_CLEAR");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clInitSys.FunLoadIniSys();
            clsDB_Proc.Initial(clInitSys.DbConfig, clInitSys.DbConfig_WMS);
            _unityContainer = new UnityContainer();
            _unityContainer.RegisterInstance(new WCSController());
            //_webApiHost = new WebApiHost(new Startup(_unityContainer), "127.0.0.1:9000");
            _webApiHost = new WebApiHost(new Startup(_unityContainer), clInitSys.WcsApi_Config.IP);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MoveTaskAddInfo info = new MoveTaskAddInfo
            {
                taskNo = txtTaskNo.Text,
                bussinessType = txtBusinessType.Text,
                locationFrom = txtSrc.Text,
                locationTo = txtDst.Text,
                WhetherAllout = txtWhetherAllOut.Text,
                lastPallet = txtLastPallet.Text,
                deliveryTime = DateTime.Now.ToString("dd-MM-yyyy"),
        };

            FunReport(info);
        }

        delegate void degShowCmdtoGrid(string str);

        public bool FunReport(MoveTaskAddInfo info)
        {
            degShowCmdtoGrid obj;
            string strJson = Newtonsoft.Json.JsonConvert.SerializeObject(info);
            //clsWriLog.Log.FunWriTraceLog_CV(strJson);
            //string sLink = $"http://127.0.0.1:9000/WCS/MoveTaskAdd";
            //string strResonse = HttpPost(sLink, strJson);

            //var add2 = $"http://127.0.0.1:9000/WCS/MOVE_TASK_ADD";
            var sLink2 = $"http://{clInitSys.WcsApi_Config.IP}/WCS/{cboFun.Text}";

            if(cboFun.Text == "MOVE_TASK_FORCE_CLEAR")
            {
                txtBusinessType.Enabled = false;
                txtSrc.Enabled = false;
                txtDst.Enabled = false;
                txtWhetherAllOut.Enabled = false;
                txtLastPallet.Enabled = false;
            }

            Task.Run(() =>
            {
                using (HttpClient client = new HttpClient())
                {
                    var result2 = client.PostAsJsonAsync(sLink2, info).Result;
                    try
                    {
                        FunTest(result2.ReasonPhrase);
                    }
                    catch (Exception ex)
                    {
                        textBox7.Text = ex.Message;
                    }
                }
            });

            return true;
        }

        public void FunTest(string str)
        {
            degShowCmdtoGrid obj;
            if (InvokeRequired)
            {
                obj = new degShowCmdtoGrid(FunTest);
                Invoke(obj, str);
            }
            else
            {
                textBox7.Text = str;
            }
        }

        //public string HttpPost(string url, RetrieveTransferInfo body)
        //{
        //    using (HttpClient client = new HttpClient())
        //    {
        //        #region 呼叫遠端 Web API
        //        HttpResponseMessage response = null;

        //        #region  設定相關網址內容
        //        // Accept 用於宣告客戶端要求服務端回應的文件型態 (底下兩種方法皆可任選其一來使用)
        //        //client.DefaultRequestHeaders.Accept.TryParseAdd("application/json");
        //        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        //        // Content-Type 用於宣告遞送給對方的文件型態
        //        //client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
        //        var fooFullUrl = $"{url}";
        //        using (var fooContent = new StringContent(body, Encoding.UTF8, "application/json"))
        //        {
        //            response = client.PostAsync(fooFullUrl, fooContent).Result;
        //            if (response.IsSuccessStatusCode)
        //            {
        //                return response.Content.ReadAsStringAsync().Result;
        //            }
        //            else if (response != null)
        //            {
        //                throw new Exception(response.Content.ReadAsStringAsync().Result);
        //            }
        //            else
        //                return "";
        //        }
        //        #endregion
        //        #endregion
        //    }
        //}
    }
}
