$M.appShop.main = function (S) {
    var win = $(document.body).addControl({
        xtype: "Window",
        text: "应用商城",
        ico: "fa-shopping-cart",
        isModal: true,
        style: { width: "450px" },
        onClose: function () {
            //if (S.onClose) S.onClose(win, rdata);
        }
    });
    //var tab = win.addControl({ xtype: "Tab", items: [{ text: "应用下载", "class": "form-horizontal" }, { text: "已安装"}] });
    var grid = win.addControl({
        xtype: "GridView",
        dock: $M.Control.Constant.dockStyle.fill, showHeader: 0, border: 0,
        style:{height:"360px"},
        columns: [{ text: "ico", name: "ico", width: 40 }, { text: "title", name: "title", visible: false }, { text: "info", name: "info" }, { name: "name", width: 80, isLink: true}],
        onCellFormatting: function (sender, e) {
            if (e.columnIndex == 0) {
                return "<img src='" + e.value + "' style='width:40px;height:40px;'>";
            } else if (e.columnIndex == 2) {
                return "<h4>" + grid.rows[e.rowIndex].cells[1].val() + "</h4><span>" + grid.rows[e.rowIndex].cells[2].val() + "</span>";
            } else if (e.columnIndex == 3) {
                return "安装";
            }
        },
        onRowCommand: function (sender, e) {
            if (e.x == 3) {
                if (grid.rows[e.y].cells[3].attr("foreColor") ==null) {
                    grid.rows[e.y].cells[3].html("安装中...");
                    grid.rows[e.y].cells[3].attr("foreColor", "#808080");
                    $M.comm("appShop.ajax.setup", { appName: grid.rows[e.y].cells[3].val() }, function () {
                        grid.rows[e.y].cells[3].html("已安装");
                    });
                }
            }
        }
    });
    win.show();
    $M.comm("appShop.ajax.readAppList", null, function (json) {
        grid.addRow(json);
    });

};