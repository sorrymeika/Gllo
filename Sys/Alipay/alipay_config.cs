﻿using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using Gllo.Sys;

namespace AlipayClass
{
    /// <summary>
    // 功能：设置帐户有关信息及返回路径（基础配置页面）
    // 版本：3.1
    // 日期：2010-11-24
    // 说明：
    // 以下代码只是为了方便商户测试而提供的样例代码，商户可以根据自己网站的需要，按照技术文档编写,并非一定要使用该代码。
    // 该代码仅供学习和研究支付宝接口使用，只是提供一个参考。

    // 如何获取安全校验码和合作身份者ID
    // 1.访问支付宝商户服务中心(b.alipay.com)，然后用您的签约支付宝账号登陆.
    // 2.访问“技术服务”→“下载技术集成文档”（https://b.alipay.com/support/helperApply.htm?action=selfIntegration）
    // 3.在“自助集成帮助”中，点击“合作者身份(Partner ID)查询”、“安全校验码(Key)查询”

    // 安全校验码查看时，输入支付密码后，页面呈灰色的现象，怎么办？
    // 解决方法：
    // 1、检查浏览器配置，不让浏览器做弹框屏蔽设置
    // 2、更换浏览器或电脑，重新登录查询。
    /// </summary>
    public class AlipayConfig
    {
        //定义变量（无需改动）
        private string partner = "";
        private string key = "";
        private string seller_email = "";
        private string return_url = "";
        private string notify_url = "";
        private string input_charset = "";
        private string sign_type = "";
        private string transport = "";
        private string show_url = "";
        private string mainname = "";

        /// <summary>
        /// 获取或设置合作者身份ID
        /// </summary>
        public string Partner
        {
            get { return partner; }
            set { partner = value; }
        }

        /// <summary>
        /// 获取或设置交易安全检验码
        /// </summary>
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        /// <summary>
        /// 获取或设置签约支付宝账号或卖家支付宝帐户
        /// </summary>
        public string Seller_email
        {
            get { return seller_email; }
            set { seller_email = value; }
        }

        /// <summary>
        /// 获取或设置付完款后跳转的页面路径
        /// </summary>
        public string Return_url
        {
            get { return return_url; }
            set { return_url = value; }
        }

        /// <summary>
        /// 获取或设置服务器异步通知页面路径
        /// </summary>
        public string Notify_url
        {
            get { return notify_url; }
            set { notify_url = value; }
        }

        /// <summary>
        /// 获取或设置字符编码格式
        /// </summary>
        public string Input_charset
        {
            get { return input_charset; }
            set { input_charset = value; }
        }

        /// <summary>
        /// 获取或设置签名方式
        /// </summary>
        public string Sign_type
        {
            get { return sign_type; }
            set { sign_type = value; }
        }

        /// <summary>
        /// 获取或设置访问模式
        /// </summary>
        public string Transport
        {
            get { return transport; }
            set { transport = value; }
        }

        /// <summary>
        /// 获取或设置网站商品的展示地址
        /// </summary>
        public string Show_url
        {
            get { return show_url; }
            set { show_url = value; }
        }

        /// <summary>
        /// 获取或设置收款方名称
        /// </summary>
        public string Mainname
        {
            get { return mainname; }
            set { mainname = value; }
        }

        public AlipayConfig()
        {
            //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

            //合作身份者ID，以2088开头由16位纯数字组成的字符串
            partner = Config.AlipayPartner;

            //交易安全检验码，由数字和字母组成的32位字符串
            key = Config.AlipayKey;

            //签约支付宝账号或卖家支付宝帐户
            seller_email = Config.AlipaySellerEmail;

            //付完款后跳转的页面 要用 http://格式的完整路径，不允许加?id=123这类自定义参数
            return_url = Config.AlipayReturnUrl;

            //交易过程中服务器通知的页面 要用 http://格式的完整路径，不允许加?id=123这类自定义参数
            notify_url = Config.AlipayNotifyUrl;

            //网站商品的展示地址，不允许加?id=123这类自定义参数
            show_url = Config.Server + "/index.html";

            //收款方名称，如：公司名称、网站名称、收款人姓名等
            mainname = "GLLO洁利来";

            //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

            //字符编码格式 目前支持 gbk 或 utf-8
            input_charset = "utf-8";

            //签名方式 不需修改
            sign_type = "MD5";

            //访问模式,根据自己的服务器是否支持ssl访问，若支持请选择https；若不支持请选择http
            transport = "http";
        }
    }
}