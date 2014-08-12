using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Gllo.BLL;
using Gllo.Sys;
using Gllo.Models;

namespace Gllo.Web.Manage.Controllers
{
    public class PrivilegeController : Controller
    {
        #region PrivilegeGroups
        public ActionResult PrivilegeGroups()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 14))
                return Redirect("/Manage/Error/2.html");

            return View();
        }
        #endregion

        #region GetPrivilegeGroups
        public ActionResult GetPrivilegeGroups()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 14))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            var res = privilegeBLL.GetPrivilegeGroups();

            return Json(new { success = true, data = res });
        }
        #endregion

        #region AddGroup
        public ActionResult AddGroup()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1401))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();
                string groupName = vld.Get("groupName", emptyAble: false, emptyText: "请填写用户组！");
                if (vld.HasError)
                {
                    return Json(new { success = false, msg = vld.GetError() });
                }

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                privilegeBLL.AddPrivilegeGroup(groupName);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1401))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region ModifyGroup
        public ActionResult ModifyGroup()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1402))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();

                PrivilegeGroupObj groupObj = new PrivilegeGroupObj();
                groupObj.GroupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请传入用户组编号！");
                groupObj.GroupName = vld.Get("groupName", emptyAble: false, emptyText: "请填写用户组！");
                if (vld.HasError)
                {
                    return Json(new { success = false, msg = vld.GetError() });
                }

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                privilegeBLL.ModifyPrivilegeGroup(groupObj);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1402))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region GetGroupByID
        public ActionResult GetGroupByID()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 14))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请传入用户组编号！");
            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            var res = privilegeBLL.GetGroupByID(groupID);

            return Json(new { success = true, data = res });

        }
        #endregion

        #region DeletePrivilegeGroup
        public ActionResult DeletePrivilegeGroup()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1403))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请传入用户组编号！");
            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            privilegeBLL.DeletePrivilegeGroup(groupID);

            return Json(new { success = true });
        }
        #endregion

        #region SetPrivilege
        public ActionResult SetPrivilege()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1404))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();

                int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请传入用户组编号！");
                string sPrivileges = vld.Get("privileges", regex: @"^\d+(,\s*\d+)*$", regexText: "传入的权限参数错误");
                if (vld.HasError)
                {
                    return Json(new { success = false, msg = vld.GetError() });
                }
                IList<int> privilegeIds = new List<int>();
                if (!string.IsNullOrEmpty(sPrivileges))
                {
                    string[] aPrivileges = sPrivileges.Split(',');
                    for (int i = 0; i < aPrivileges.Length; i++)
                    {
                        string id = aPrivileges[i].Trim();
                        if (!string.IsNullOrEmpty(id))
                        {
                            privilegeIds.Add(int.Parse(id));
                        }
                    }
                }

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                privilegeBLL.SetPrivileges(groupID, privilegeIds);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1404))
                    return Redirect("/Manage/Error/2.html");

                return View();
            }
        }
        #endregion

        #region GetPrivileges
        public ActionResult GetPrivileges()
        {
            Validation vld = new Validation();

            int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请传入用户组编号！");
            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            var res = privilegeBLL.GetPrivileges();
            var privileges = privilegeBLL.GetGroupPrivileges(groupID);

            return Json(new { success = true, data = res, privileges = privileges });
        }
        #endregion

        #region Users
        public ActionResult Users()
        {
            if (!AppData.IsManagerLogin)
                return Redirect("/Manage/Error/1.html");
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 15))
                return Redirect("/Manage/Error/2.html");

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            ViewBag.groups = privilegeBLL.GetPrivilegeGroups();

            return View();
        }
        #endregion


        #region GetUsers
        public ActionResult GetUsers()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 15))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            int groupID = vld.GetInt("groupID");
            string userName = vld.Get("userName");
            int page = vld.GetInt("page");
            int pageSize = vld.GetInt("pageSize");
            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            int total;
            var res = privilegeBLL.GetUsers(userName, groupID, page, pageSize, out total);

            return Json(new { success = true, data = res, total = total });
        }
        #endregion

        #region AddUser
        public ActionResult AddUser()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1501))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();

                int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请选择用户组！");
                string userName = vld.Get("userName", emptyAble: false, emptyText: "请填写用户名！");
                string password = vld.Get("password", emptyAble: false, emptyText: "请填写密码！");
                if (vld.HasError)
                {
                    return Json(new { success = false, msg = vld.GetError() });
                }
                UserBLL userBLL = new UserBLL();
                if (userBLL.IsRegist(userName))
                {
                    return Json(new { success = false, msg = "该用户名已被使用！" });
                }

                password = Md5.MD5(password);

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                privilegeBLL.AddUser(userName, password, groupID);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1501))
                    return Redirect("/Manage/Error/2.html");

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                ViewBag.groups = privilegeBLL.GetPrivilegeGroups();
                return View();
            }
        }
        #endregion

        #region ModifyUser
        public ActionResult ModifyUser()
        {
            if (Request.HttpMethod == "POST")
            {
                if (!AppData.IsManagerLogin)
                    return Json(new { success = false, msg = "您未登录后台或会话已过期" });
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1502))
                    return Json(new { success = false, msg = "您没有执行该操作的权限" });

                Validation vld = new Validation();

                int userID = vld.GetInt("userID", emptyAble: false, emptyText: "参数不完整！");
                string userName = vld.Get("userName", emptyAble: false, emptyText: "请填写用户名！");
                int groupID = vld.GetInt("groupID", emptyAble: false, emptyText: "请选择用户组！");
                if (vld.HasError)
                {
                    return Json(new { success = false, msg = vld.GetError() });
                }

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                privilegeBLL.ModifyUser(userID, userName, groupID);

                return Json(new { success = true });
            }
            else
            {
                if (!AppData.IsManagerLogin)
                    return Redirect("/Manage/Error/1.html");
                if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1502))
                    return Redirect("/Manage/Error/2.html");

                PrivilegeBLL privilegeBLL = new PrivilegeBLL();
                ViewBag.groups = privilegeBLL.GetPrivilegeGroups();
                return View();
            }
        }
        #endregion

        #region GetUserByID
        public ActionResult GetUserByID(int userID)
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 15))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            var res = privilegeBLL.GetUserByID(userID);

            return Json(new { success = true, data = res });
        }
        #endregion

        #region DeleteUser
        public ActionResult DeleteUser()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });
            if (PrivilegeBLL.HasNotPrivilege(AppData.SessionUserID, 1503))
                return Json(new { success = false, msg = "您没有执行该操作的权限" });

            Validation vld = new Validation();

            int userID = vld.GetInt("userID", emptyAble: false, emptyText: "请传入用户组编号！");
            if (vld.HasError)
            {
                return Json(new { success = false, msg = vld.GetError() });
            }

            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            privilegeBLL.DeleteUser(userID);

            return Json(new { success = true });
        }
        #endregion


        public ActionResult GetUserPrivileges()
        {
            if (!AppData.IsManagerLogin)
                return Json(new { success = false, msg = "您未登录后台或会话已过期" });

            int userID = AppData.SessionUserID;
            PrivilegeBLL privilegeBLL = new PrivilegeBLL();
            var res = privilegeBLL.GetUserPrivileges(userID);
            return Json(new { success = true, data = res });
        }

    }
}
