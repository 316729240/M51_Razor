$M.dataBase.backupData = function () {
    var myDate = new Date();
    $M.prompt("备份名称",
                    function (value) {
                        $M.app.call("$M.dataBase.backup", { name: value });
                    },
                    { vtype: { required: true, isRightfulString: true, value: myDate.format("yyyyMMddhhmmss")} });
};
$M.dataBase.main = function (S) {
    var tab = mainTab.find("dataBase");
    if (tab) { tab.focus(); return; }
    tab = mainTab.addItem({ text: "数据库管理", name: "dataBase", ico: "fa-database", closeButton: true });
    var toolBar = tab.addControl({ xtype: "ToolBar", items: [
        [{ text: "执行脚本", ico: "fa-exclamation", onClick: function () {
            loadPage(sqlScript.val(), 1);
        }
        }],
        [{ text: "备份数据库", onClick: function () {
            $M.dataBase.backupData();
        }
        }, { text: "还原数据库", onClick: function () {
            $M.app.call("$M.dataBase.backupRestore", null);
        }
        }]]
    });
    var frame = tab.addControl({ xtype: "Frame", type: "x", dock: 2, items: [{ size: 200, text: "表", ico: "fa-table" }, { size: "*"}] });
    var tableTree = frame.items[0].addControl({
        xtype: "TreeView", dock: 2,
        onMouseDoubleClick: function (sender, e) {
            var tableName = sender.selectedItem.attr("tableName");
            if (tableName) {
                var sqlstr = "select * from [" + tableName + "]";
                sqlScript.val(sqlstr);
                loadPage(sqlScript.val(), 1);
            }
        }
    });
    tableTree.root.addItem([{ text: "自定义表" }, { text: "系统表"}]);
    var sqlScript = frame.items[1].addControl({ xtype: "TextBox", rows: 3, multiLine: true });
    var dataList = frame.items[1].addControl({
        xtype: "GridView", dock: $M.Control.Constant.dockStyle.fill, border: 1, condensed: 1
    });
    tab.focus();
    $M.comm("dataBase.ajax.readTableList", {}, function (json) {
        for (var i = 0; i < json.length; i++) {
            if (json[i][0] == "1") {
                tableTree.root.items[0].addItem({ text: json[i][1], tableName: json[i][2] });
            } else {
                tableTree.root.items[1].addItem({ text: json[i][1], tableName: json[i][2] });
            }
        }
    });
    var pageBar = frame.items[1].addControl({
        xtype: "PageBar",
        onPageChanged: function (sender, e) {
            //item._pageNo = e.pageNo;
            //loadPage(item._pageNo, item._orderByName, item._sortDirection, item._type);
        }
    });
    var loadPage = function (sql, pageNo) {
        $M.comm("dataBase.ajax.dataList", {
            sql: sql,
            pageNo: pageNo
        },
            function (json) {
                dataList.clear();
                dataList.attr("columns", json[0]);
                dataList.addRow(json[1].data);
                pageBar.attr("pageSize", json[1].pageSize);
                pageBar.attr("recordCount", json[1].recordCount);
                pageBar.attr("pageNo", json[1].pageNo);
                frame.resize();
            });
    };
    //loadPage("select u_content from article", 1);
};