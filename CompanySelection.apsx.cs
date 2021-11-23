using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Web.Configuration;
using YTL.Common;
using System.Data;
using System.Data.SqlClient;
using NLog;
using System.IO;
//Entity-Resource-Module
using APSCv4.Class;
using APSCv4.Resources;
using APSCv4.Entity;
using APSCv4.Modules;

namespace APSCv4.Operation
{
    public partial class CompanySelection : System.Web.UI.Page
    {
        #region Declaration

        private ResourcesEntity resEntity = new ResourcesEntity();
        private GeneralEntity reqEntity = new GeneralEntity();
        private Master_Company mdlEntity = new Master_Company();
        private Logger _Logger = LogManager.GetCurrentClassLogger();
        private string Conn = "";
        string sErr = string.Empty;
        List<Control> lst_controls = new List<Control>();

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            Conn = CommonFunction.GetConnectionSystem();

            if (!Page.IsPostBack)
            {
                
                if (Request.QueryString["Action"] != null && Request.QueryString["Action"].ToString() == "FindControl")
                {
                    string getList = string.Empty;
                    //PageCtrlAccess(ref getList);
                    CommonFunction.ValidateCtrl(this.Master.FindControl("cphContent").Page, ref getList);
                    HttpContext.Current.Response.Write(getList);
                    HttpContext.Current.Response.End();
                }

                Initial();
                BindMain();
            }   
        }

        #region Page Control Retriever - NOT USED : ALREADY CONVERT TO CLASS LEVEL
        
        private void PageCtrlAccess(ref string getList)
        {
            string ctrlName = string.Empty;
                string ctrlType = string.Empty;
                string ctrlID = string.Empty;
                string ctrlList = string.Empty;
                ctrlName = "dvGrid";
                ctrlType = "div";

                this.Master.FindControl("cphContent").FindControl(ctrlName).Visible = true;
                RetrieveAllControls(this.Master.FindControl("cphContent").Page);
                var ignore = new List<string> { "lbl", "Head", "Page" };
                foreach (Control contrl in lst_controls)
                {
                    if (contrl.ID != null)
                    {
                        ctrlID = contrl.ID;
                        var hasAny = ignore.Any(ctrlID.Contains);

                        //if (!ctrlID.Contains("lbl") && !ctrlID.Contains("Head") && !ctrlID.Contains("Page") && !ctrlID.Contains("ScriptManager") && !ctrlID.Contains("updProgress"))
                        if (!hasAny)
                        {
                            ctrlList = ctrlList + contrl.ID + "/";
                        }
                    }
                        
 
                }

                getList = ctrlList;

                //CommonFunction.MessageBox(ctrlList, upList);
       }
        
        private void RetrieveAllControls(Control control)
        {
            
            foreach (Control ctr in control.Controls)
            {
                if (ctr != null)
                {

                    lst_controls.Add(ctr);
                    if (ctr.HasControls())
                    {
                        RetrieveAllControls(ctr);

                    }
                }
            }

        }

        #endregion

        #region Initialize
        private void Initial()
        {
            DataTable dt = new DataTable();
            reqEntity.UserID = Convert.ToString(Session["UserID"]);
            dt = mdlEntity.BindUserRole(reqEntity, Conn);
            if (dt.Rows.Count > 0)
            {
                Session["UserRole"] = dt.Rows[0]["UserRole"];
            }

        }
        #endregion

        #region GvMain

        private void BindMain()
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                DataView dv = default(DataView);
                reqEntity.UserID = Convert.ToString(Session["UserID"]);
                dvGridProj.Visible = false;
                dt = mdlEntity.BindCompMaster(reqEntity, Conn);

                ds.Tables.Add(dt);
                dv = ds.Tables[0].DefaultView;

                gvMain.DataSource = dv;
                gvMain.DataBind();

            }
            catch (Exception e)
            {
                _Logger.Error(e, "Bind comp. selection gridview => BindMain");
                throw;
            }
        }

        protected void gvMain_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
            }

            int i = 0;
            int colNo = 0;
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState == DataControlRowState.Normal || e.Row.RowState == DataControlRowState.Alternate || e.Row.RowState == DataControlRowState.Selected || e.Row.RowState == (DataControlRowState.Alternate | DataControlRowState.Selected | DataControlRowState.Edit)))
            {
                string strEmail = string.Empty;
                colNo = gvMain.Columns.Count;
                e.Row.Attributes.Add("onmouseover", "this.style.cursor='auto'");

                for (i = 0; i <= colNo - 1; i++)
                {
                    e.Row.Cells[i].Attributes.Add("style", "word-break:break-all;word-wrap:break-word");
                }

                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                }
                else
                {
                    if (e.Row.RowState != DataControlRowState.Edit)
                    {

                        e.Row.Attributes.Add("onmouseover", "this.style.cursor='pointer'");
                        e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvMain, "Select$" + e.Row.RowIndex);

                    }
                }

            }

        }

        protected void gvMain_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string CompCode = (gvMain.SelectedRow.FindControl("lblCompCode") as Label).Text;
            string CompID = (gvMain.SelectedRow.FindControl("lblRecID") as Label).Text;
            
            Session["CompCode"] = Convert.ToString(CompCode).Trim();
            Session["CompID"] = Convert.ToString(CompID).Trim();
            Session["CompType"] = Convert.ToString(CompCode).Trim();
            Session["APSProject"] = null;

            BindSub();
            gvSub.SelectedIndex = -1;
            
        }



        #endregion

        #region PreRender
        protected override void OnPreRender(EventArgs e)
        {
            GridViewRow pagerRowSub = gvSub.BottomPagerRow;

            if (pagerRowSub != null && pagerRowSub.Visible == false)
            {
                pagerRowSub.Visible = true;
            }

            base.OnPreRender(e);
        }
        #endregion

        #region Page ViewState
        private void PageViewState()
        {
            ViewState["SortDirectionSub"] = string.Empty;
            ViewState["filterSub"] = string.Empty;
        }
        #endregion

        #region Refresh Gridview
        private void RefreshgvSub()
        {
            string vSortExp = string.Empty;
            string vSortDir = string.Empty;
            string vFilter = string.Empty;

            GetGVValueSub(ref vSortExp, ref vSortDir, ref vFilter);
            BindSub(vSortExp, vSortDir, vFilter);
        }
        #endregion

        #region GV Data
        private void GetGVValueSub(ref string vSortExp, ref string vSortDir, ref string vFilter)
        {
            if (ViewState["SortExpressionSub"] != null)
            {
                vSortExp = Data.GetString((string)ViewState["SortExpressionSub"]);
            }

            if (ViewState["SortDirectionSub"] != null)
            {
                vSortDir = Data.GetString((string)ViewState["SortDirectionSub"]);
            }

            if (ViewState["filterSub"] != null)
            {
                vFilter = Data.GetString((string)ViewState["filterSub"]);
            }
        }
        #endregion

        #region gvSub

        protected void gvSub_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            string PCode = (gvSub.SelectedRow.FindControl("lblProjCode") as Label).Text;
            string Mth = (gvSub.SelectedRow.FindControl("lblMonth") as Label).Text;
            Session["APSProject"] = Convert.ToString(PCode);
            Session["Month"] = Convert.ToString(Mth);

            //Label lblPCode = (Label)this.Master.FindControl("lblPostingLocation");
            //lblPCode.Text = Convert.ToString(PCode);

        }

        private void BindSub(string sortExp = "", string sortDir = "", string filterString = "")
        {
            try
            {
                DataSet ds = new DataSet();
                DataTable dt = new DataTable();
                DataView dv = default(DataView);
                reqEntity.UserID = Convert.ToString(Session["UserID"]);
                reqEntity.CompID = Convert.ToInt32(Session["CompID"]);

                dvGridProj.Visible = true;

                dt = mdlEntity.BindProjMaster(reqEntity, Conn);

                ds.Tables.Add(dt);
                dv = ds.Tables[0].DefaultView;

                if (!string.IsNullOrEmpty(sortExp))
                {
                    dv.Sort = string.Format("{0} {1}", sortExp, sortDir);
                }

                if (!string.IsNullOrEmpty(filterString))
                {
                    dv.RowFilter = filterString;
                }

                gvSub.DataSource = dv;
                gvSub.DataBind();

            }
            catch (Exception e)
            {
                _Logger.Error(e, "Bind project selection gridview => BindSub");
                throw;
            }
        }

        protected void gvSub_RowCreated(object sender, GridViewRowEventArgs e)
        {
            //** For sorting image ****

            if (e.Row.RowType == DataControlRowType.Header)
            {
                int sortColumnIndex = GetSortColumnIndex();
                if (sortColumnIndex != -1)
                {
                    AddSortImage(sortColumnIndex, e.Row);
                }
            }
            //**End: For sorting image ****

        }

        private int GetSortColumnIndex()
        {

            foreach (DataControlField field in gvSub.Columns)
            {
                if (field.SortExpression == (string)ViewState["SortExpressionSub"])
                {
                    return gvSub.Columns.IndexOf(field);
                }
            }
            return -1;
        }

        private void AddSortImage(int columnIndex, GridViewRow headerRow)
        {
            // Create the sorting image based on the sort direction.
            Image sortImage = new Image();
            if (ViewState["SortDirectionSub"].ToString() == "asc")
            {
                sortImage.ImageUrl = Data.GetString((string)WebConfigurationManager.AppSettings["grvImgAsc"]);
                sortImage.AlternateText = "Ascending Order";
                sortImage.ImageAlign = ImageAlign.AbsBottom;

                sortImage.ToolTip = "Asc";
            }
            else
            {
                sortImage.ImageUrl = Data.GetString((string)WebConfigurationManager.AppSettings["grvImgDesc"]);
                sortImage.ImageAlign = ImageAlign.TextTop;
                sortImage.AlternateText = "Descending Order";
                sortImage.ToolTip = "Desc";
            }

            // Add the image to the appropriate header cell.
            headerRow.Cells[columnIndex].Controls.Add(sortImage);
        }

        protected void gvSub_Sorting(object sender, GridViewSortEventArgs e)
        {
            gvSub.EditIndex = -1;
            gvSub.SelectedIndex = -1;
            string sortExpression = e.SortExpression;

            ViewState["SortExpressionSub"] = sortExpression;

            BindSub(e.SortExpression, SortingDirection, Data.GetString((string)ViewState["filterSub"]));

        }

        public string SortingDirection
        {
            get
            {

                if (Convert.ToString(ViewState["SortDirectionSub"]) == "desc")
                {
                    ViewState["SortDirectionSub"] = "asc";

                }
                else
                {

                    ViewState["SortDirectionSub"] = "desc";
                }

                return Data.GetString((string)ViewState["SortDirectionSub"]);
            }
            set { ViewState["SortDirectionSub"] = value; }
        }

        protected void gvSub_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSub.PageIndex = e.NewPageIndex;
            RefreshgvSub();
        }

        protected void gvSub_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
            }

            int i = 0;
            int colNo = 0;
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState == DataControlRowState.Normal || e.Row.RowState == DataControlRowState.Alternate || e.Row.RowState == DataControlRowState.Selected || e.Row.RowState == (DataControlRowState.Alternate | DataControlRowState.Selected | DataControlRowState.Edit)))
            {

                colNo = gvMain.Columns.Count;
                e.Row.Attributes.Add("onmouseover", "this.style.cursor='auto'");

                for (i = 0; i <= colNo - 1; i++)
                {
                    e.Row.Cells[i].Attributes.Add("style", "word-break:break-all;word-wrap:break-word");
                }

                if ((e.Row.RowState & DataControlRowState.Edit) > 0)
                {
                }
                else
                {
                    if (e.Row.RowState != DataControlRowState.Edit)
                    {

                        e.Row.Attributes.Add("onmouseover", "this.style.cursor='pointer'");
                        e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvSub, "Select$" + e.Row.RowIndex);

                    }
                }

            }

            if (e.Row.RowType == DataControlRowType.Footer)
            {
                Label lblRowsCount = (Label)e.Row.FindControl("lblRowsCount");

                int iTotalRecords = ((DataView)gvSub.DataSource).Count;
                int iEndRecord = gvSub.PageSize * (gvSub.PageIndex + 1);
                int iStartsRecords = iEndRecord - gvSub.PageSize;

                if (iEndRecord > iTotalRecords)
                {
                    iEndRecord = iTotalRecords;
                }

                if (iStartsRecords == 0)
                {
                    iStartsRecords = 1;
                }
                if (iStartsRecords > 1)
                {
                    iStartsRecords = iStartsRecords + 1;
                }
                if (iEndRecord == 0)
                {
                    iEndRecord = iTotalRecords;
                }

                lblRowsCount.Text = iStartsRecords + " - " + iEndRecord.ToString() + " of " + iTotalRecords.ToString();
            }

        }

        #endregion

        #region Page Action

        protected void btnSearch_Onclick(object sender, EventArgs e)
        {
            Filter();
            gvSub.SelectedIndex = -1;
        }

        private void Filter()
        {
            string strFilter = string.Empty;
            string FProj = string.Empty;

            FProj = Convert.ToString(txtProjCode.Text);

            if (!String.IsNullOrEmpty(FProj))
            {
                strFilter = strFilter + " AND Project LIKE '%" + Data.EscapeLikeValue(Data.GetString(FProj)) + "%' ";
            }

            if (!String.IsNullOrEmpty(strFilter))
            {
                ViewState["filterSub"] = strFilter.Substring(4);
            }
            else
            {
                ViewState["filterSub"] = string.Empty;
            }

            gvSub.PageIndex = 0;
            RefreshgvSub();

        }

        #endregion
    }
}
