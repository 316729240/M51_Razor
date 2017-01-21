<%@  Language="C#"  %>
<%
    using (Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Server.MapPath(@"~\m5.zip")))
    {
        zip.ExtractAll(Server.MapPath(@"~\"),Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
    } 
%>