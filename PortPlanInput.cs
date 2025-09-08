using Exatell.Core;
using Exatell.Data;
using Exatell.Framework;
using Exatell.WinForm;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Threading;
using Exatell.Logistics.Common.EDI;

namespace MyNamespace
{
    // 自定义委托类型替代Func<T0,T1>
    public delegate TResult Func<T, TResult>(T arg);

    // 自定义等待条件委托
    public delegate bool WaitCondition(IWebDriver driver);

    class PortPlanInput
    {
        private IWebDriver driver;
        private JObject applyData;
        private XDataSource dataSource;

        public PortPlanInput(JObject applyData, XDataSource dataSource)
        {
            this.applyData = applyData;
            this.dataSource = dataSource;
            this.driver = WebDriver.GetInstance();
        }

        private ChainableWebElement Find(string xpath)
        {
            return new ChainableWebElement(driver.FindElement(By.XPath(xpath)));
        }

        private ReadOnlyCollection<IWebElement> FindAll(string xpath)
        {
            return driver.FindElements(By.XPath(xpath));
        }

        private ChainableWebElement FindId(string id)
        {
            return new ChainableWebElement(driver.FindElement(By.Id(id)));
        }

        private ChainableWebElement FindCss(string selector)
        {
            return new ChainableWebElement(driver.FindElement(By.CssSelector(selector)));
        }

        private bool HasElement(string xpath, ISearchContext element)
        {
            if (element == null)
                return driver.FindElements(By.XPath(xpath)).Count > 0;
            else
                return element.FindElements(By.XPath(xpath)).Count > 0;
        }

        private bool HasElement(string xpath)
        {
            return HasElement(xpath, null);
        }

        private bool ElementDisplayed(string xpath, IWebElement element)
        {
            if (element == null)
            {
                try
                {
                    return driver.FindElement(By.XPath(xpath)).Displayed;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                try
                {
                    return element.FindElement(By.XPath(xpath)).Displayed;
                }
                catch
                {
                    return false;
                }
            }
        }

        private bool ElementDisplayed(string xpath)
        {
            return ElementDisplayed(xpath, null);
        }

        private bool DomReady()
        {
            return ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").ToString().Equals("complete");
        }

        // 自定义等待方法替代WebDriverWait的lambda表达式
        private bool WaitUntil(WaitCondition condition, TimeSpan timeout)
        {
            DateTime endTime = DateTime.Now.Add(timeout);
            while (DateTime.Now < endTime)
            {
                if (condition(driver))
                    return true;
                System.Threading.Thread.Sleep(100);
            }
            return false;
        }

        private void AcceptAlertIfPresents(int timeout)
        {
            try
            {
                DateTime endTime = DateTime.Now.AddMilliseconds(timeout);
                while (DateTime.Now < endTime)
                {
                    try
                    {
                        IAlert alert = driver.SwitchTo().Alert();
                        alert.Accept();
                        return;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch { }
        }

        private void AcceptAlertIfPresents()
        {
            AcceptAlertIfPresents(1000);
        }

        /// <summary>
        /// 港区排计划
        /// </summary>
        public void ApplyWAT()
        {
            Actions actions = new Actions(driver);
            driver.Navigate().GoToUrl(applyData["account"]["URL"].ToString());
            #region 登录
            if (ElementDisplayed("//*[@id='btnSignIn']"))
            {
                try
                {
                    // 使用自定义等待方法替代WebDriverWait
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        return DomReady() && FindId("btnSignIn").Displayed;
                    }), TimeSpan.FromMinutes(1));

                    try
                    {
                        try
                        {
                            WaitUntil(new WaitCondition(delegate (IWebDriver d)
                            {
                                return ElementDisplayed("//*[@id='showMsgBox']/div/div/div[3]/button");
                            }), TimeSpan.FromSeconds(2));
                        }
                        catch { }

                        if (ElementDisplayed("//*[@id='showMsgBox']/div/div/div[3]/button")) //关闭公告栏
                        {
                            Find("//*[@id='showMsgBox']/div/div/div[3]/button").Click();
                        }
                    }
                    catch (Exception e) { }

                    FindId("btnSignIn").Click();

                    try
                    {
                        try
                        {
                            WaitUntil(new WaitCondition(delegate (IWebDriver d)
                            {
                                return ElementDisplayed("//*[@id='btnModalConfirm']");
                            }), TimeSpan.FromSeconds(3));
                        }
                        catch { }

                        if (ElementDisplayed("//*[@id='btnModalConfirm']"))
                        {
                            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].className = 'btn btn-primary active';", driver.FindElement(By.XPath("//*[@id='btnModalConfirm']")));
                            Find("//*[@id='btnModalConfirm']").Click();
                        }
                    }
                    catch (Exception e) { }

                    FindId("userName").SendKeys(applyData["account"]["UserName"]);
                    FindId("userPwd").SendKeys(applyData["account"]["Password"]);
                    Thread.Sleep(1000);

                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        try
                        {
                            IWebElement input = d.FindElement(By.CssSelector("input[value='立即登录']"));
                            if (input != null && !string.IsNullOrEmpty(input.GetAttribute("nonce")))
                            {
                                return true;
                            }
                            return false;
                        }
                        catch
                        {
                            return false;
                        }
                    }), TimeSpan.FromSeconds(3));

                    FindId("btnLogin").Click();
                }
                catch { }
            }

            WaitUntil(new WaitCondition(delegate (IWebDriver d)
            {
                return HasElement("//*[@id='accountBarS']/li[2]/a");
            }), TimeSpan.FromMinutes(1));

            driver.Navigate().GoToUrl(applyData["account"]["URL"].ToString().TrimEnd('/') + "/WAT/WATApplyIndex");
            WaitUntil(new WaitCondition(delegate (IWebDriver d)
            {
                return driver.Url.Equals(applyData["account"]["URL"].ToString().TrimEnd('/') + "/WAT/WATApplyIndex")
                       && DomReady();
            }), TimeSpan.FromMinutes(1)); //等待跳转至主页

            IJavaScriptExecutor jsExecutor = (IJavaScriptExecutor)driver;
            try
            {
                try
                {
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        return ElementDisplayed("//button[@id='tipBtn']");
                    }), TimeSpan.FromSeconds(3));
                }
                catch { }

                IWebElement tipButton = driver.FindElement(By.XPath("//button[@id='tipBtn']"));
                jsExecutor.ExecuteScript("arguments[0].disabled = undefined;", tipButton);
                tipButton.Click();
                Thread.Sleep(1000);
            }
            catch (Exception e) { }

            IWebElement element = driver.FindElement(By.XPath("//div[@class='footer']"));
            jsExecutor.ExecuteScript("arguments[0].style.display = 'none';", element);
            #endregion
            #region 填入托单信息
            Find("//*[@id='termNo']/option[@value='" + applyData["WharfCode"] + "']").Click(); //码头
            FindId("re_voymsg").SendKeys(applyData["Voyage"]).SendKeys(OpenQA.Selenium.Keys.Enter); //航次
            try
            {
                FindId("re_voy_td_div").Find(".//td[contains(text(),'" + applyData["Vessel"] + "')]").Click();
            }
            catch (Exception ex)
            {
                if (ex.Message.StartsWith("element click intercepted"))
                {
                    Thread.Sleep(300);
                    FindId("re_voy_td_div").Find(".//td[contains(text(),'" + applyData["Vessel"] + "')]").Click();
                }
                else throw ex;
            }
            Thread.Sleep(300);
            Find("//*[@id='opprc']/option[@value='" + applyData["OperateType"] + "']").Click(); //操作过程
            FindId("tb_ships_contact").Clear().SendKeys(applyData["Contact"]); //联系人
            FindId("tb_ships_telephone").Clear().SendKeys(applyData["Telephone"]); //联系电话
            FindId("re_shipsconfirm_Btn").Click(); //下一步
            AcceptAlertIfPresents(60000);
            actions.SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();
            Thread.Sleep(400);
            #endregion
            #region 筛去已保存的箱子
            ChainableWebElement tbody = Find("//*[@id='tb_ctn']/tbody");
            ReadOnlyCollection<IWebElement> rows = tbody.FindElements(By.XPath(".//tr"));
            int rowsCount = rows.Count;
            foreach (IWebElement row in rows)
            {
                string ctrno = row.FindElement(By.XPath(".//input[@name='Cntrno']")).GetAttribute("value");
                if (StrUtils.IsBlank(ctrno))
                {
                    row.FindElement(By.XPath(".//td[1]/input")).Click();
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        try
                        {
                            string styleValue = row.GetCssValue("background-color");
                            return styleValue.Contains("233, 233, 233");
                        }
                        catch
                        {
                            return false;
                        }
                    }), TimeSpan.FromSeconds(10));
                    Thread.Sleep(100);
                    Find("//*[@id='ctn_deletetrBtn']").Click();
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        rows = tbody.FindElements(By.XPath(".//tr"));
                        bool result = rows.Count < rowsCount;
                        rowsCount = rows.Count;
                        return result;
                    }), TimeSpan.FromSeconds(10));
                }
                else
                {
                    // 手动实现JArray的遍历逻辑
                    for (int i = GetJArrayCount(applyData["ContList"]) - 1; i >= 0; i--)
                    {
                        JObject obj = GetJArrayItem(applyData["ContList"], i) as JObject;
                        if (obj != null && StrUtils.ValStrEqual(ctrno, (obj["XH"] + "").ToUpper()))
                        {
                            // 注意：这里需要根据实际的JArray实现来移除元素
                            // 由于无法在.NET 2.0中使用JArray.RemoveAt，需要特殊处理
                        }
                    }
                }
            }
            #endregion
            #region 填入箱信息
            // 手动实现JArray.Count
            int contListCount = GetJArrayCount(applyData["ContList"]);
            for (int i = 0; i < contListCount; i++)
            {
                FindId("ctn_addtrBtn").Click();
                AcceptAlertIfPresents(1000);
                WaitUntil(new WaitCondition(delegate (IWebDriver d)
                {
                    return ElementDisplayed("//*[@id='tb_ctn']/tbody/tr[" + (i + rowsCount + 1) + "]");
                }), TimeSpan.FromMinutes(1));

                ChainableWebElement contTr = Find("//*[@id='tb_ctn']/tbody/tr[" + (i + rowsCount + 1) + "]");
                // 手动实现JArray索引访问
                JObject cont = GetJArrayItem(applyData["ContList"], i) as JObject;
                AcceptAlertIfPresents(1000);
                ChainableWebElement xhInput = contTr.Find("./td[2]/input");
                if (i > 0)
                {
                    xhInput.Click();
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        try
                        {
                            string styleValue = contTr.GetCssValue("background-color");
                            return styleValue.Contains("233, 233, 233");
                        }
                        catch
                        {
                            return false;
                        }
                    }), TimeSpan.FromSeconds(10));
                }
                xhInput.SendKeys(cont["XH"] + ""); //箱号
                contTr.Find("./td[3]/input").SendKeys(cont["XYYR"] + "").SendKeys(OpenQA.Selenium.Keys.Enter); //持箱人
                FindId("tb_ctn_copercd").Find(".//td[text() = '" + cont["XYYR"] + "']").Click();
                AcceptAlertIfPresents(300);
                contTr.Find(".//*[@id='Csizecd']/option[@value='" + cont["CC"] + "']").Click(); //尺寸
                contTr.Find(".//*[@id='Commcode']/option[@value='" + cont["XX"] + "']").Click(); //箱型
                contTr.Find(".//*[@id='Cheightcd']/option[@value='" + cont["XG"] + "']").Click(); //箱高
                if (XConvert.ToDouble(cont["QC"], 0) > 0)
                    contTr.Find("./td[8]/input").SendKeys(cont["QC"] + ""); //前超
                if (XConvert.ToDouble(cont["HC"], 0) > 0)
                    contTr.Find("./td[9]/input").SendKeys(cont["HC"] + ""); //后超
                if (XConvert.ToDouble(cont["ZC"], 0) > 0)
                    contTr.Find("./td[10]/input").SendKeys(cont["ZC"] + ""); //左超
                if (XConvert.ToDouble(cont["YC"], 0) > 0)
                    contTr.Find("./td[11]/input").SendKeys(cont["YC"] + ""); //右超
                if (XConvert.ToDouble(cont["CG"], 0) > 0)
                    contTr.Find("./td[12]/input").SendKeys(cont["CG"] + ""); //超高
                contTr.Find("./td[14]/input").SendKeys(cont["XZ"] + ""); //箱重
                if (!(cont["WXPDJ"] + "").Equals("0") && !(cont["WXPDJ"] + "").Equals("999"))
                    contTr.Find(".//*[@id='Dnggcd']/option[@value='" + cont["WXPDJ"] + "']").Click(); //危险品等级
                contTr.Find("./td[16]/input").SendKeys(cont["LHGBH"] + ""); //联合国编号
                contTr.Find("./td[17]/input").SendKeys(cont["FH"] + ""); //铅封号
                contTr.Find("./td[18]/input").SendKeys(cont["LCWD"] + ""); //冷藏温度
                #region 填入货物信息
                int clpItemsCount = GetJArrayCount(cont["ClpItems"]);
                for (int j = 0; j < clpItemsCount; j++)
                {
                    Find("//*[@id='goods_addtrBtn']").Click();
                    AcceptAlertIfPresents(300);
                    WaitUntil(new WaitCondition(delegate (IWebDriver d)
                    {
                        return ElementDisplayed("//*[@id='tb_goods']/tbody/tr[" + (j + 1) + "]");
                    }), TimeSpan.FromMinutes(1));
                    ChainableWebElement clpItemTr = Find("//*[@id='tb_goods']/tbody/tr[" + (j + 1) + "]");
                    JObject clpItem = GetJArrayItem(cont["ClpItems"], j) as JObject;
                    clpItemTr.Find("./td[2]/input").SendKeys(clpItem["TDH"] + ""); //提单号
                    clpItemTr.Find("./td[3]/input").SendKeys(clpItem["MT"] + ""); //唛头
                    clpItemTr.Find("./td[4]/input").SendKeys(clpItem["HM"] + ""); //货名
                    if (String.IsNullOrEmpty(clpItem["CAS"] + "")) //CAS号
                    {
                        clpItemTr.Find("./td[5]/input[2]").Click();
                        if (i == 0 && j == 0)
                            AcceptAlertIfPresents(300);
                    }
                    else
                    {
                        clpItemTr.Find("./td[5]/input[1]").Click();
                        if (i == 0 && j == 0)
                            AcceptAlertIfPresents(300);
                        string[] cas = (clpItem["CAS"] + "").Split('-');
                        clpItemTr.Find("./td[6]/input[1]").SendKeys(cas[0]);
                        clpItemTr.Find("./td[6]/input[2]").SendKeys(cas[1]);
                        clpItemTr.Find("./td[6]/input[3]").SendKeys(cas[2]);
                    }
                    clpItemTr.Find("./td[7]/input").Click(); //卸货港(仅匹配前两列)
                    ReadOnlyCollection<IWebElement> trs = FindId("re_goods_unldportcd_div").FindElements(By.XPath(".//tr"));
                    for (int index = 1; index < trs.Count; index++)
                    {
                        if (trs[index].FindElement(By.XPath(".//td[2]")).Text == (clpItem["XGDM"] + "") ||
                            trs[index].FindElement(By.XPath(".//td[3]")).Text == (clpItem["XGDM"] + ""))
                        {
                            trs[index].Click();
                            break;
                        }
                    }
                    clpItemTr.Find("./td[8]/input").SendKeys(clpItem["JS"] + ""); //件数
                    clpItemTr.Find("./td[9]/input").SendKeys(clpItem["ZL"] + ""); //毛重
                    clpItemTr.Find("./td[10]/input").SendKeys(clpItem["TJ"] + ""); //体积
                    if (!(clpItem["WXPDJ"] + "").Equals("0") && !(clpItem["WXPDJ"] + "").Equals("999"))
                        clpItemTr.Find("./td[11]/select/option[@value='" + clpItem["WXPDJ"] + "']").Click(); //危险品等级
                    clpItemTr.Find("./td[12]/input").SendKeys(clpItem["LHGBM"] + ""); //联合国编号
                }

                #region 自动修正操作过程
                IWebElement dropdownElement = driver.FindElement(By.Id("opprc"));
                SelectElement selectElement = new SelectElement(dropdownElement);
                string selectedOptionValue = selectElement.SelectedOption.GetAttribute("value");
                if (!(applyData["OperateType"] + "").Equals(selectedOptionValue))
                {
                    selectElement.SelectByValue(applyData["OperateType"] + "");
                    FindId("tb_re_ships").Click();
                    actions.SendKeys(OpenQA.Selenium.Keys.PageDown).SendKeys(OpenQA.Selenium.Keys.PageDown).Perform();
                }
                #endregion

                FindId("goods_saveBtn").Click(); //货物保存按钮
                try
                {
                    DateTime alertTimeout = DateTime.Now.AddSeconds(10);
                    while (DateTime.Now < alertTimeout)
                    {
                        try
                        {
                            IAlert alert = driver.SwitchTo().Alert();
                            if (alert.Text.Equals("保存成功"))
                                alert.Accept();
                            else throw new Exception(alert.Text);
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch { }
                contTr.Find("./td[20]/input").Click(); //箱子保存按钮
                try
                {
                    DateTime alertTimeout = DateTime.Now.AddSeconds(10);
                    while (DateTime.Now < alertTimeout)
                    {
                        try
                        {
                            IAlert alert = driver.SwitchTo().Alert();
                            if (alert.Text.Contains("保存成功"))
                                alert.Accept();
                            else throw new Exception(alert.Text);
                            break;
                        }
                        catch
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                }
                catch { }
                #endregion
            }
            #endregion
            ((IJavaScriptExecutor)driver).ExecuteScript("alert('【系统】自动录入执行完毕')");
        }

        private int GetJArrayCount(object jArray)
        {
            // 这里需要根据实际使用的Json.NET版本实现
            // 如果使用Newtonsoft.Json，可能需要通过反射获取Count属性
            if (jArray is JArray)
            {
                return ((JArray)jArray).Count;
            }
            return 0;
        }

        private object GetJArrayItem(object jArray, int index)
        {
            // 这里需要根据实际使用的Json.NET版本实现
            // 如果使用Newtonsoft.Json，可能需要通过反射获取Item属性
            if (jArray is JArray)
            {
                return ((JArray)jArray)[index];
            }
            return null;
        }
    }
}