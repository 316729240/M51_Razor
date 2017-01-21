$M.Control["PageBar"] = function (BoxID, S, CID) {
    var A = null, T = this;
    var pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
    var record1 = ((S.pageNo - 1) * S.pageSize) + 1;
    var record2 = record1 + S.pageSize - 1;
    var firstPage = function () {
        pageChanged(0);
    };
    var prePage = function () {
        S.pageNo--;
        pageChanged(1);
    };
    var nextPage = function () {
        S.pageNo++;
        pageChanged(2);
    };
    var lastPage = function () {
        S.pageNo = pageCount;
        pageChanged(3);
    };
    var pageChanged = function (type) {
        if (S.onPageChanged) S.onPageChanged(T, { pageNo: S.pageNo });
    };
    var bar = BoxID.addControl({ xtype: "ToolBar", items: [
                        [{ xtype: "Button", name: "firstButton", ico: "fa-fast-backward", onClick: firstPage }, { xtype: "Button", name: "preButton", ico: "fa-backward", onClick: prePage}],
                        [{ xtype: "Label", text: "第", style: { float: "left", padding: "4px 0px 0px 0px"}}],
                        [{ xtype: "TextBox", name: "pageNo", style: { width: "50px" }, size: 1, text: S.pageNo, onKeyDown: function (sender, e) {
                            if (e.which == 13) {
                                S.pageNo = sender.val();
                                if (isNaN(S.pageNo)) {
                                    S.pageNo = 1;
                                    sender.val(1);
                                }
                                if (S.pageNo > pageCount) S.pageNo = pageCount;
                                if (S.pageNo < 1) S.pageNo = 1;

                                pageChanged(3);
                            }
                        }
                        }],
                        [{ xtype: "Label", name: "pageCount", text: "页，共页", style: { float: "left", padding: "4px 0px 0px 0px"}}],
                        [{ xtype: "Button", name: "nextButton", ico: "fa-forward", onClick: nextPage }, { xtype: "Button", name: "lastButton", ico: "fa-fast-forward", onClick: lastPage}],
                        [{ xtype: "Button", ico: "fa-refresh", onClick: function () { pageChanged(3); } }],
                        { xtype: "Label", name: "recordCount", text: "当前到条 共条", style: { float: "right", padding: "4px 0px 0px 0px"} }
                        ]
    });
    var firstButton = bar.find("firstButton");
    var preButton = bar.find("preButton");
    var nextButton = bar.find("nextButton");
    var lastButton = bar.find("lastButton");
    var pageNoText = bar.find("pageNo");
    var pageCountLabel = bar.find("pageCount");
    var recordCountLabel = bar.find("recordCount");
    T.css = function (style) { bar.css(style); };
    T.height = function (h) { return bar.height(h); };
    T.attr = function (a, b) {
        S[a] = b;
        if (a == "pageNo") {
            pageNoText.val(b);
            firstButton.enabled(b > 1);
            preButton.enabled(b > 1);
            nextButton.enabled(b < pageCount);
            lastButton.enabled(b < pageCount);

            pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
            record1 = ((S.pageNo - 1) * S.pageSize) + 1;
            record2 = record1 + S.pageSize - 1;
            pageCountLabel.html("页，共" + pageCount + "页");
            recordCountLabel.html("当前" + record1 + "到" + record2 + "条 共" + S.recordCount + "条");


        }
        else if (a == "recordCount") {
            pageCount = Math.floor(((S.recordCount - 1) / S.pageSize) + 1);
            record1 = ((S.pageNo - 1) * S.pageSize) + 1;
            record2 = record1 + S.pageSize - 1;
            pageCountLabel.html("页，共" + pageCount + "页");
            recordCountLabel.html("当前" + record1 + "到" + record2 + "条 共" + b + "条");
        }
        return b;
    };
    T.go = function (number) {
        S.pageNo = number;
        pageChanged(3);
    };

};
