<script type="text/javascript">
        //<![CDATA[
        function CheckLogin() {
            var strLoginID = document.getElementById('<%=txtUserID.ClientID%>').value
            var strPwd = document.getElementById('<%=txtPwd.ClientID%>').value;
            if (strLoginID == "") {
                document.getElementById('<%=txtUserID.ClientID%>').focus();
                alert("Please key in Login ID!");
                return false;
            }
            if (strPwd == "") {
                document.getElementById('<%=txtPwd.ClientID%>').focus();
                alert("Please key in Password!");
                return false;
            }
            return true;
        }
        //]]>
    </script>
