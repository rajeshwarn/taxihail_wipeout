<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Config.aspx.cs" Inherits="apcurium.MK.Web.Config" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel ID="LoginPanel" Visible="true" runat="server">
            <table>
                <tr>
                    <td>
                        <asp:TextBox ID="txtUsername" runat="server"></asp:TextBox>
                    </td>
                    <td>
                        <asp:TextBox ID="txtPassword" runat="server"></asp:TextBox>
                    </td>
                </tr>
                <tr>
                    <td colspan="2" align="right">
                        <asp:Button ID="cmdLogin" runat="server" Text="Login" OnClick="cmdLogin_Click" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
        <asp:Panel ID="SettingsPanel" Visible="false" runat="server">
            <asp:DataList ID="configList" runat="server" DataKeyField="Key" EnableViewState="False">
                <ItemTemplate>
                    <div style="vertical-align: middle;">
                        <asp:Label Width="250px" ID="KeyLabel" runat="server" Text='<%# Eval("Key") %>' />
                        <asp:TextBox ID="ValueText" Width="350px" runat="server" Text='<%# Eval("Value") %>'> </asp:TextBox>
                    </div>
                </ItemTemplate>
                <AlternatingItemTemplate>
                    <div style="background-color:#cccccc; vertical-align: middle;" >
                        <asp:Label Width="250px" ID="KeyLabel" runat="server" Text='<%# Eval("Key") %>' />
                        <asp:TextBox ID="ValueText" Width="350px" runat="server" Text='<%# Eval("Value") %>'> </asp:TextBox>
                    </div>
                </AlternatingItemTemplate>
            </asp:DataList>
            <br />
            <table width="600px">
                <tr>
                    <td align="right">
                        <asp:Button ID="cmdRedirect" runat="server" Text="Open Data Page" OnClick="cmdRedirect_Click" />
                        &nbsp;&nbsp;
                        <asp:Button ID="cmdSave0" runat="server" Text="Save Settings" OnClick="cmdSave_Click" />
                    </td>
                </tr>
            </table>
        </asp:Panel>
    </div>
    </form>
</body>
</html>
