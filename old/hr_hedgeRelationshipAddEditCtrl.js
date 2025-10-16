

app.controller('hedgeRelationshipAddEditCtrl', [
    '$scope', '$haService', 'ngDialog', '$q', '$timeout', '$compile', '$http', '$interval',
    function ($scope, $haService, ngDialog, $q, $timeout, $compile, $http, $interval) {

        $scope.AnalyticsErrorCount = -1;
        $scope.ha_errors = [];
        $scope.relationshipUserMessage = "";

        var id = searchUrlParameter('id');

        if (id === '') {
            id = 0;
        }

        var clientID = searchUrlParameter('clientID');

        if (clientID === '') {
            clientID = 0;
        }

        $scope.hedge_relationships = [];
        $scope.enums = {};
        $scope.HedgeRelationshipItemType = 'None';
        $scope.EffectivenessMethods = [];
        $scope.HedgeInceptionMemoTemplates = [];
        $scope.Currencies = [];
        $scope.RegressionTestResults = [];
        $scope.openDetailsTab = true;
        $scope.HedgeRelationshipActivities = [];

        $scope.buildRegressionTable = true;

        function setDropDownListBenchmark() {
            var benchmarks = [];
            $scope.DropDownList.BenchmarkList = $scope.enums["Benchmark"];
            var benchmarkList = $scope.DropDownList.BenchmarkList;

            if ($scope.Model.HedgeType === "CashFlow") {
                var notCFBenchmarks = ["FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15"];
                benchmarkList.map(function (v) {
                    if (notCFBenchmarks.indexOf(v.Value) === -1) {
                        benchmarks.push(v);
                    }
                });
            }
            else {
                var notFVBenchmarks = ["FFUTFDTR", "FHLBTopeka", "USDTBILL4WH15", "Other", "Prime"];
                benchmarkList.map(function (v) {
                    if (notFVBenchmarks.indexOf(v.Value) === -1) {
                        benchmarks.push(v);
                    }
                });
            }

            $scope.DropDownList.BenchmarkList = benchmarks;
        };

        $scope.init = function (modelId, callback) {

            if (id != modelId) {
                id = modelId;
            }

            $timeout(function () {
                jQuery("#haaAppContainer input").each(function (e) {
                    jQuery(this).trigger("blur");
                });
            }, 10);

            var enum_ = $haService.setUrl("Enum").getOld();
            var method_ = $haService.setUrl("EffectivenessMethod").getOld();
            var currency_ = $haService.setUrl("Currency").getOld();
            var hr_ = $haService.setUrl("HedgeRelationship").setId(id).getOld($scope);
            var template_ = $haService.setUrl("InceptionMemoTemplate").getOld();

            $q.all([enum_, method_, currency_, hr_, template_]).then(function (responses) {
                $scope.enums = responses[0].data;

                $scope.DropDownList = {
                    HedgedItemTypeList: $scope.enums['HedgedItemType'],
                    BenchmarkList: $scope.enums['Benchmark'],
                    StandardList: $scope.enums['Standard'],
                    HedgeRiskTypeList: $scope.enums['HedgeRiskType'],
                    HedgeDirectionList: $scope.enums['HedgeDirection'],
                    HedgeTypeList: $scope.enums['HRHedgeType'],
                    FairValueMethodList: $scope.enums['FairValueMethod'],
                    InEffMeasurementList: $scope.enums['InEffMeasurement'],
                    ReportingFrequencyList: $scope.enums['ReportingFrequency'],
                    PeriodSizeList: $scope.enums['PeriodSize'],
                    ActionList: [{ "Value": "Designate", "Disabled": false }, { "Value": "De-Designate", "Disabled": false }, { "Value": "Re-Designate", "Disabled": false }],
                    TabActionList: [{ "Value": "Amortization" }, { "Value": "Option Amortization" }],
                    PaymentHolidaysList: $scope.enums['FinancialCenter_Edge'],
                    PaymentFrequencyList: $scope.enums['PaymentFrequency_Edge'],
                    DayCountConvList: $scope.enums['DayCountConv_Edge'],
                    AssetLiabilityList: $scope.enums['AssetLiability'],
                    PayBusDayConvList: $scope.enums['PayBusDayConv_Edge'],
                    AmortizationMethodList: $scope.enums['AmortizationMethod'],
                    HedgeExposureList: $scope.enums['HedgeExposure'],
                    AccountingTreatmentList: $scope.enums['HedgeAccountingTreatment'],
                    IntrinsicMethodList: $scope.enums['IntrinsicMethod'],
                    HedgingInstrumentStructureList: $scope.enums['HedgingInstrumentStructure'],
                };


                setDropDownListHedgeRiskType();
                $scope.Model.HedgeRiskType = 'None';

                setDropDownListBenchmark();

                $scope.EffectivenessMethods = responses[1].data;
                setDropDownListEffectivenessMethods();

                $scope.DropDownList.Currencies = [];
                $scope.DropDownList.Currencies.push({
                    "ShortName": "",
                    "LongName": "None"
                });

                responses[2].data.map(function (v) {
                    $scope.DropDownList.Currencies.push(v);
                });

                setModelData(responses[3].data, true);
                setDropDownListHedgingInstrumentStructure();
                jQuery("#haaAppContainer").show();
                setWorkFlow();

                if (callback != null && callback != undefined) {
                    callback();
                }

                $scope.HedgeInceptionMemoTemplates = responses[4].data;
                setDropDownListInceptionMemoTemplate();

                setDropDownListAmortizationMethod();

                $scope.DropDownList.ReportingFrequencyList.map((v) => {
                    v.Disabled = v.Value !== "Monthly";
                });

                $scope.DropDownList.PeriodSizeList.map((v) => {
                    v.Disabled = v.Value === "None" || v.Value === "Quarter";
                });
            });
        };

        function setDropDownListEffectivenessMethods() {

            if ($scope != undefined && $scope.Model !== undefined && $scope.Model.HedgeType !== 'FairValue') {
                $scope.Model.FairValueMethod = 'None';
            }

            $scope.DropDownList.EffectivenessMethods = [];

            $scope.EffectivenessMethods.map(function (v) {
                if ($scope != undefined && $scope.Model !== undefined && ((($scope.Model.HedgeType === 'FairValue' && v.IsForFairValue)
                    || $scope.Model.HedgeType !== 'FairValue')) && !$scope.Model.IsAnOptionHedge) {

                    $scope.DropDownList.EffectivenessMethods.push({
                        "ID": v.ID.toString(),
                        "Name": v.Name,
                        "Disabled": v.ID.toString() !== "1"
                    });
                }
                else if (v.Name.includes('Regression - Change in') && $scope.Model.IsAnOptionHedge) {
                    $scope.DropDownList.EffectivenessMethods.push({
                        "ID": v.ID.toString(),
                        "Name": v.Name
                    });
                    return;
                }

                if ($scope != undefined && $scope.Model !== undefined && !$scope.Model.IsAnOptionHedge && v.Name === 'Regression - Change in Intrinsic Value') {
                    $scope.DropDownList.EffectivenessMethods.pop();
                    return;
                }
            });
        }

        function setDropDownListInceptionMemoTemplate() {

            if ($scope != undefined && $scope.Model !== undefined) {

                $scope.DropDownList.HedgeInceptionMemoTemplates = [];

                $scope.HedgeInceptionMemoTemplates.map(function (v) {
                    if ($scope != undefined && $scope.Model !== undefined) {

                        $scope.DropDownList.HedgeInceptionMemoTemplates.push({
                            "ID": v.ID.toString(),
                            "Name": v.Name,
                            "Disabled": v.ID.toString() === "0" || v.ID.toString() === "1"
                        });
                    }
                });
            }
        }

        $scope.filterIfOption = function (selected, index, array) {
            if ($scope.Model.IsAnOptionHedge) {
                return selected.Value === "Option Amortization";
            }
            else {
                return true;
            }
        }

        $scope.$watch('Model.InceptionMemoTemplateID', function (new_, old_) {
            if (new_ !== undefined && new_ !== old_) {
                $scope.HedgeInceptionMemoTemplates.map(function (v) {
                    if ($scope != undefined && $scope.Model !== undefined) {
                        if (v.ID.toString() == new_) {
                            var html = v.TemplateDesc !== undefined && v.TemplateDesc !== null ? v.TemplateDesc : '';
                            setRichTextBoxControl('init', 'Objective', html);
                            setRichTextBoxControl('detail', 'Objective', html);
                        }
                    }
                });
            }
        });


        $scope.$watch('Model.HedgeRiskType', function (new_, old_) {
            if (new_ !== undefined && new_ !== old_) {
                setBenchmarkLabel();
                setDropDownListHedgeType();
                setDropDownListExposure();
                setBenchmarkContractualRateExposure();
            }
        });

        $scope.$watch('Model.HedgeType', function (new_, old_) {
            if (new_ !== undefined && new_ !== old_) {
                setBenchmarkLabel();
                setDropDownListBenchmark();
                setDropDownListEffectivenessMethods();
                setDropDownListExposure();
                setBenchmarkContractualRateExposure();

                if ($scope.Model.HedgeType === 'NetInvestment') {
                    setDropDownListAccountingTreatment();
                }
            }
        });

        $scope.$watch('Model.HedgeExposure', function (new_, old_) {
            if (new_ !== undefined && new_ !== old_) {
                setDropDownListHedgedItemType();
            }
        });

        function setBenchmarkLabel() {

            if (($scope.Model.HedgeRiskType === 'InterestRate')
                && ($scope.Model.HedgeType === 'CashFlow')) {
                $scope.Model.BenchMarkLabel = 'Contractual Rate';
            }
            else if (($scope.Model.HedgeRiskType === 'InterestRate')
                && ($scope.Model.HedgeType === 'FairValue')) {
                $scope.Model.BenchMarkLabel = 'Benchmark';
            }
            else {
                $scope.Model.BenchMarkLabel = 'Benchmark';
            };

            $('#idBenchmark').attr('data-placeholder', $scope.Model.BenchMarkLabel);
        };

        function setBenchmarkContractualRateExposure() {

            if (($scope.Model.HedgeRiskType === 'ForeignExchange')
                && ($scope.Model.HedgeType === 'CashFlow')) {
                $scope.Model.Benchmark = '0';
                $scope.Model.ExposureCurrency = null;
                $scope.Model.HedgeAccountingTreatment = '0';
            }
            else if (($scope.Model.HedgeRiskType === 'ForeignExchange')
                && ($scope.Model.HedgeType === 'FairValue')) {
                $scope.Model.Benchmark = '0';
                $scope.Model.ExposureCurrency = null;
                $scope.Model.HedgeAccountingTreatment = '0';
            }
            else if (($scope.Model.HedgeRiskType === 'ForeignExchange')
                && ($scope.Model.HedgeType === 'NetInvestment')) {
                $scope.Model.Benchmark = '0';
                $scope.Model.HedgeExposure = '0';
            }
            else if ($scope.Model.HedgeRiskType === 'InterestRate') {
                $scope.Model.HedgeExposure = '0';
                $scope.Model.ExposureCurrency = null;
                $scope.Model.HedgeAccountingTreatment = '0';
            }
        };

        function setDropDownListHedgeRiskType() {
            var hedgerisk = [];
            $scope.DropDownList.HedgeRiskTypeList.map(function (v) {
                if ((v.Value != 'MarketPrice')
                    && (v.Value != 'Credit')) {
                    hedgerisk.push(v);
                }
            });
            $scope.DropDownList.HedgeRiskTypeList = hedgerisk;
        };

        function setDropDownListHedgeType() {
            var hedgetype = [];

            if ($scope != undefined && $scope.Model !== undefined && $scope.Model.HedgeRiskType === 'InterestRate') {
                $scope.DropDownList.HedgeTypeList.map(function (v) {
                    if (v.Value != 'NetInvestment') {
                        hedgetype.push(v);
                    }
                });
                $scope.DropDownList.HedgeTypeList = hedgetype;
            }
            else {
                $scope.DropDownList.HedgeTypeList = [];

                $scope.DropDownList.HedgeTypeList = $scope.enums['HRHedgeType'];

                $scope.DropDownList.HedgeTypeList.map(function (v) {
                    hedgetype.push(v);
                });
            }
        };


        function setDropDownListExposure() {
            var exposure = [];
            $scope.DropDownList.HedgeExposureList = [];
            $scope.DropDownList.HedgeExposureList = $scope.enums['HedgeExposure'];

            if ($scope != undefined && $scope.Model !== undefined && $scope.Model.HedgeRiskType === 'ForeignExchange') {

                if ($scope.Model.HedgeType === 'FairValue') {
                    $scope.DropDownList.HedgeExposureList.map(function (v) {
                        if ((v.Value != 'ForecastedExposure')
                            && (v.Value != 'IntraEntity')) {
                            exposure.push(v);
                        }
                    });
                    $scope.DropDownList.HedgeExposureList = exposure;
                }
                else if ($scope.Model.HedgeType === 'CashFlow') {
                    $scope.DropDownList.HedgeExposureList.map(function (v) {
                        if (v.Value != 'UnrecognizedFirmCommitment') {
                            exposure.push(v);
                        }
                    });
                    $scope.DropDownList.HedgeExposureList = exposure;
                }
                else {

                    $scope.DropDownList.HedgeExposureList.map(function (v) {
                        exposure.push(v);
                    });
                }
            }
            else {
                $scope.DropDownList.HedgeExposureList = null;
            };
        };

        function setDropDownListHedgedItemType() {
            var item = [];

            if ($scope != undefined && $scope.Model !== undefined && $scope.Model.HedgeExposure === 'RecognizedAssetLiability') {
                $scope.DropDownList.HedgedItemTypeList.map(function (v) {
                    if ((v.Text != 'None')
                        && (v.Text != 'Forecasted')) {
                        item.push(v);
                    }
                });
                $scope.DropDownList.HedgedItemTypeList = item;
            }
            else {
                $scope.DropDownList.HedgedItemTypeList = [];
                $scope.DropDownList.HedgedItemTypeList = $scope.enums['HedgedItemType'];
                $scope.DropDownList.HedgedItemTypeList.map(function (v) {
                    item.push(v);
                });
            }
        };

        function setDropDownListAccountingTreatment() {
            var item = [];
            $scope.DropDownList.AccountingTreatmentList = [];
            $scope.DropDownList.AccountingTreatmentList = $scope.enums['HedgeAccountingTreatment'];
            $scope.DropDownList.AccountingTreatmentList.map(function (v) {
                item.push(v);
            });
        };

        function setDropDownListAmortizationMethod() {
            var amortization = [];
            $scope.DropDownList.AmortizationMethodList.map(function (v) {
                if (v.Value != 'IntrinsicValueMethod') {
                    amortization.push(v);
                }
            });
            $scope.DropDownList.AmortizationMethodList = amortization;
        };

        function setDropDownListHedgingInstrumentStructure() {
            
            if (!$scope.Model.HedgingInstrumentStructureText)
            {
                $scope.Model.HedgingInstrumentStructureText = 'Single Instrument';
                $scope.Model.HedgingInstrumentStructure = "SingleInstrument";
            }

            if ($scope.IsDPIUser)
            {
                return;
            }

            $scope.DropDownList.HedgingInstrumentStructureList = $scope.DropDownList.HedgingInstrumentStructureList
                .filter(function (e) { return e.Value === 'SingleInstrument' });
        };

        $scope.$watch('Model.IsAnOptionHedge', function (new_) {
            if (new_ !== undefined) {
                if (!new_) {
                    $scope.Model.AmortizeOptionPremimum = false;
                    $scope.Model.IsDeltaMatchOption = false;
                    $scope.Model.ExcludeIntrinsicValue = false;
                }
                if (!$scope.Model.OffMarket) {
                    $scope.Model.OffMarket = false;
                }
            }
        });

        $scope.$watch('Model.ExcludeIntrinsicValue', function (new_) {
            if (new_ !== undefined) {
                if (!new_) {
                    $scope.Model.IntrinsicMethod = "None";
                    $scope.Model.AmortizeOptionPremimum = false;
                    $scope.Model.IsDeltaMatchOption = false;
                }
            }
        });

        $scope.$watch('Model.HedgeState', function (new_, old_) {
            if (new_ !== undefined) {
                setWorkFlow();

                if ($scope.Model.HedgeState === "Dedesignated") {
                    $scope.Model.IsAnOptionHedge = false;
                }
            }
        });


        setWorkFlow = function () {
            if ($scope.Model.HedgeState === 'Draft') {
                $scope.DropDownList.ActionList.splice(1, 1);
            }

            if ($scope.Model.HedgeState !== 'Designated' || $scope.Model.HedgeType !== "CashFlow") { /*DE-3928: Show Re-Designate on designated relationship only*/
                $scope.DropDownList.ActionList.splice(2, 1);
            }

            if ($scope.Model.HedgeState === 'Designated' ||
                $scope.Model.HedgeState === "Dedesignated") { /*DE-2731: Add ability to Re-Draft a hedge relationship in De-Designated status*/
                $scope.DropDownList.ActionList.splice(0, 1);
                $scope.DropDownList.ActionList.splice(0, 0, { "Value": "Redraft", "Disabled": false });
            }

            $scope.DropDownList.ActionList.map(function (v) {
                v.Disabled = !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'));
            });
        };

        retreiveGLAccounts = function (scope) {
            $haService
                .setUrl('GLAccounts/GetForHedging/' + scope.Model.ClientID + '/' + scope.Model.BankEntityID)
                .get()
                .then(function (response) {
                    $scope.DropDownList.GLAccounts = response.data;
                });
        };

        setModelData = function (response, isInit) {
            var today = new Date();
            var valueDate = (today.getMonth() + 1).toString() + '/' + today.getDate().toString() + '/' + today.getFullYear().toString();

            if (response !== undefined && response !== null || (response && response.ID > 0)) {
                $scope.Model = response;
                $scope.Model.ClientID = $scope.Model.ClientID.toString();
                $scope.Model.BankEntityID = $scope.Model.BankEntityID.toString();

                if ($scope.Model.ProspectiveEffectivenessMethodID !== undefined)
                    $scope.Model.ProspectiveEffectivenessMethodID = $scope.Model.ProspectiveEffectivenessMethodID.toString();

                if ($scope.Model.RetrospectiveEffectivenessMethodID !== undefined)
                    $scope.Model.RetrospectiveEffectivenessMethodID = $scope.Model.RetrospectiveEffectivenessMethodID.toString();

                if ($scope.Model.HedgeExposure !== undefined)
                    $scope.Model.HedgeExposure = $scope.Model.HedgeExposure.toString();

                if ($scope.Model.ExposureCurrency !== undefined)
                    $scope.Model.ExposureCurrency = $scope.Model.ExposureCurrency.toString();

                if ($scope.Model.HedgeAccountingTreatment !== undefined)
                    $scope.Model.HedgeAccountingTreatment = $scope.Model.HedgeAccountingTreatment.toString();

                if ($scope.Model.InceptionMemoTemplateID !== undefined) {
                    $scope.Model.InceptionMemoTemplateID = $scope.Model.InceptionMemoTemplateID.toString();
                }
                else {
                    $scope.Model.InceptionMemoTemplateID = "0";
                }

                $scope.Model.InEffMeasurement = "None";

                $scope.CanEditPreIssuanceHedge = $scope.Model.HedgeState === 'Draft';
                $scope.CanEditPortfolioLayerMethod = $scope.Model.HedgeState === 'Draft';

                id = $scope.Model.ID;

                if ($scope.RegressionRunDate) {
                    //DE-4534
                    var analyticsExceptionLength = 0;
                    $scope.Model.HedgeRegressionBatchesLogs.map(function (x) {
                        analyticsExceptionLength += x.HedgeRelationshipLog.Logs.filter(function (y) { return y.Key == 'AnalyticsException' && moment(new Date(y.CreatedOn)) >= $scope.RegressionRunDate }).length;
                    });
                    $scope.RegressionRunDate = null;

                    $scope.AnalyticsErrorCount = analyticsExceptionLength;

                    if ($scope.AnalyticsErrorCount > 0) {
                        $scope.ha_errors.push("Analytics Exception");
                    }

                }

            } else {

                $scope.Model = {
                    GenerateInception: false,
                    TaxPurposes: true,
                    IncludeCva: false,
                    AssetLiability: "None",
                    HedgeRiskType: "None",
                    HedgeDirection: "All",
                    HedgedItemType: "None",
                    Benchmark: "None",
                    HedgeType: "None",
                    Standard: "ASC815",
                    FairValueMethod: "None",
                    AmortizationMethod: "None",
                    ProspectiveEffectivenessMethodID: "0",
                    RetrospectiveEffectivenessMethodID: "0",
                    InEffMeasurement: "None",
                    ReportingFrequency: "None",
                    PeriodSize: "Month",
                    ClientID: "0",
                    BankEntityID: "0",
                    HedgeState: "Draft", //text is needed since only a display
                    HedgeStateText: "Draft", //text is needed since only a display
                    HedgedItems: [],
                    HedgingItems: [],
                    Observation: 0,
                    ID: 0,
                    PeriodicChanges: true,
                    DesignationDate: valueDate,
                    Shortcut: false,
                    AmortizeOptionPremimum: false,
                    IsAnOptionHedge: false,
                    IsDeltaMatchOption: true,
                    OptionPremium: 0,
                    HedgeExposure: "None",
                    HedgeAccountingTreatment: "None",
                    InceptionMemoTemplateID: "None",
                    AvailableForSale: false,
                    IntrinsicMethod: "None",
                    ExcludeIntrinsicValue: false,
                    IsHAOptIntEnabled: false,
                    OffMarket: false
                };
            }

            $scope.Model.BenchMarkLabel = 'Benchmark';
            $scope.Model.ValueDate = valueDate;
            $scope.IsDPIUser = isCurrentUserDpiUser();

            $scope.onActionChangeValue = "Workflow";
            $scope.openDetailsTab = $scope.Model.ID > 0 ? true : false;

            $scope.tabsInitialization();
            $scope.tablesInitialization();
            $scope.generateSnapshots(isInit);

            $scope.DraftDesignatedIsDPIUser = $scope.Model.HedgeState === 'Draft' || ($scope.Model.HedgeState === 'Designated' && ($scope.IsDPIUser || $scope.Model.ContractType === 'SaaS' || $scope.Model.ContractType === 'SwaS'));
            $scope.DraftIsDPIUser = $scope.Model.HedgeState === 'Draft' && $scope.IsDPIUser;
            $scope.DesignatedIsDPIUser = $scope.Model.HedgeState === 'Designated' && !($scope.IsDPIUser || $scope.Model.ContractType === 'SaaS' || $scope.Model.ContractType === 'SwaS');
            $scope.Model.IsNewHedgeDocumentTemplate = $scope.Model.Objective === undefined || $scope.Model.Objective && $scope.Model.Objective.match(/^(?:<\/?p>|<\/?br\s*\/?>|<\/?div>|<\/?span>|\s|&nbsp;)*$/);
            $scope.DedesignatedHedgeDocument = $scope.Model.HedgeState === 'Dedesignated' && $scope.Model.HedgeDocumentTemplateName;

            setBenchmarkLabel();
            setNotional($scope);
            retreiveGLAccounts($scope);
            $scope.setTextBoxFromFields();

            if ($scope.Model.AmortizationMethod === undefined
                || $scope.Model.AmortizationMethod === null) {
                $scope.Model.AmortizationMethod = "None";
            }

            if (clientID > 0) {
                $scope.Model.ClientID = clientID.toString();
            }

            if (!$scope.$$phase) {
                $scope.$digest();
            }
        };

        $scope.tabsInitialization = function () {
            jQuery("#tabs-hedgeRelationship").tabs({
                activate: function (event, ui) {
                    var x = $(ui.newPanel).attr("id");
                    if (x === "tabs-hedgeRelationship-2") {
                        if ($scope.buildRegressionTable) {
                            $scope.buildRegressionTable = false;
                            $scope.RegressionTestResultsTableInit();
                        }
                    }
                    else if (x === "tabs-hedgeRelationship-6"
                        && $scope.Model.HedgeRelationshipOptionTimeValues.length === 0) {
                        $scope.openOptionTimeValueAmortDialog();
                    }
                }
            });
        };

        RemoveItem = function (type) {

            var gridObj = $("#" + type + "Div").data("ejGrid");
            var row = gridObj.getSelectedRecords()[0];

            if (row != null) {
                $scope.Model[type + 's'] = $scope.Model[type + 's'].filter(function (el) {
                    if (el.ItemID !== row.ItemID) {
                        return el;
                    }
                });
            }

            var obj = $("#" + type + "Div").ejGrid("instance");
            obj.dataSource($scope.Model[type + 's']);

            setNotional($scope);
        };

        $scope.tablesInitialization = function () {
            var cols = [
                { headerText: "Hedged Item ID", field: "ItemID", isPrimaryKey: true },
                { headerText: "Description", field: "Description", width: 380 },
                { headerText: "Notional", field: "Notional", format: "{0:C2}", textAlign: "right", headerTextAlign: "right" },
                { headerText: "Fixed Rate", field: "Rate", textAlign: "right", headerTextAlign: "right" },
                { headerText: "Credit Spread", field: "Spread", textAlign: "right", headerTextAlign: "right" },
                { headerText: "Start Date", field: "EffectiveDate", textAlign: "right", headerTextAlign: "right" },
                { headerText: "Maturity Date", field: "MaturityDate", textAlign: "right", headerTextAlign: "right" },
                { headerText: "Trade Status", field: "ItemStatusText" },
                {
                    headerText: "", width: 100,
                    commands: [{
                        type: ej.Grid.UnboundType.Delete,
                        buttonOptions: {
                            text: "Remove",
                            cssClass: "hedginghedgeditemDelete"/*, click: $scope.removeTheHedgingItem*/,
                            click: function () {
                                RemoveItem('HedgedItem');
                            }
                        }
                    }]
                }
            ];

            if ($scope.hideSelectNewOrRemoveTrade()) {
                cols.splice(-1);
                $scope.showSelectNewTrade = true;
                if (!$scope.$$phase) {
                    $scope.$apply();
                }
            }

            $("#HedgedItemDiv").ejGrid({
                dataSource: ej.DataManager($scope.Model.HedgedItems),
                columns: cols,
                rowDataBound: function (args) {
                    $(args.row[0].cells[3]).html(((parseFloat(args.rowData.Rate) * 100).toFixed(5)) + "%");
                    $(args.row[0].cells[4]).html(((parseFloat(args.rowData.Spread) * 10000).toFixed(3)) + "bps");
                },
                recordDoubleClick: function (row) {
                    if (row.data !== null) {
                        var id = row.data.ItemID;
                        var itemType = row.data.HedgeRelationshipItemType;
                        var securityType = row.data.SecurityType;
                        $scope.openExistingTrade(id, itemType);
                    }
                },
                locale: "de-DE",
                allowSelection: true,
                enableAltRow: false,
                enableRowHover: true,
                isResponsive: true,
                allowSorting: true,
                allowSearching: true
            });

            cols = [
                { headerText: "Hedging Item ID", field: "ItemID", isPrimaryKey: true },
                { headerText: "Description", field: "Description", width: 380 },
                { headerText: "Notional", field: "Notional", format: "{0:C2}", textAlign: "right" },
                { headerText: "Fixed Rate", field: "Rate", textAlign: "right" },
                { headerText: "Credit Spread", field: "Spread", textAlign: "right" },
                { headerText: "Start Date", field: "EffectiveDate", textAlign: "right" },
                { headerText: "Maturity Date", field: "MaturityDate", textAlign: "right" },
                { headerText: "Trade Status", field: "ItemStatusText" },
                {
                    headerText: "", width: 100,
                    commands: [{
                        type: ej.Grid.UnboundType.Delete,
                        buttonOptions: {
                            text: "Remove",
                            cssClass: "hedginghedgeditemDelete"/*, click: $scope.removeTheHedgingItem*/,
                            click: function () {
                                RemoveItem('HedgingItem');
                            }
                        }
                    }]
                }
            ];

            if ($scope.hideSelectNewOrRemoveTrade()) {
                cols.splice(-1);
                $scope.showSelectNewTrade = true;
                if (!$scope.$$phase) {
                    $scope.$apply();
                }
            }

            $("#HedgingItemDiv").ejGrid({
                dataSource: ej.DataManager($scope.Model.HedgingItems),
                columns: cols,
                rowDataBound: function (args) {
                    $(args.row[0].cells[3]).html(((parseFloat(args.rowData.Rate) * 100).toFixed(5)) + "%");
                    $(args.row[0].cells[4]).html(((parseFloat(args.rowData.Spread) * 10000).toFixed(3)) + "bps");
                },
                recordDoubleClick: function (row) {
                    if (row.data !== null) {
                        var id = row.data.ItemID;
                        var itemType = row.data.HedgeRelationshipItemType;
                        var securityType = row.data.SecurityType;
                        $scope.openExistingTrade(id, itemType, securityType);
                    }
                },
                locale: "de-DE",
                allowSelection: true,
                enableAltRow: false,
                enableRowHover: true,
                isResponsive: true,
                allowSorting: true,
                allowSearching: true
            });

            $scope.RegressionTestResultsTableInit();
            $scope.initAmortizationDiv();
            $scope.initAmortizationDiv1();
        };

        function InitializeHedgeRelationshipOptionTimeValueAmort(defaultGlAccountId, defaultContraAccountId) {
            $scope.HedgeRelationshipOptionTimeValueAmort = {
                ID: 0,
                GLAccountID: defaultGlAccountId,
                ContraAccountID: defaultContraAccountId,
                FinancialCenters: ["USGS"],
                PaymentFrequency: "Monthly",
                DayCountConv: "ACT_360",
                PayBusDayConv: "Preceding",
                AdjDates: true
            };
        }

        function OpenAmortizationDialog() {
            ngDialog.open({
                template: 'ha_amortization',
                scope: $scope,
                className: 'ngdialog-theme-default ngdialog-theme-custom custom-height-800 amortizationAddEditDialog',
                title: 'Add/Edit Amortization',
                showTitleCloseshowClose: true,
                width: "500px",
                closeByEscape: false,
                closeByDocument: false,
                preCloseCallback: function () {
                    var totalAmountElement = document.getElementById('totalAmount');
                    if (totalAmountElement) {
                        totalAmountElement.removeEventListener('input', handleTotalAmountInput);
                    }
                    return true;
                }
            });

            function waitForElement(selector, callback) {
                var element = document.getElementById(selector);
                if (element) {
                    callback(element);
                } else {
                    $timeout(function () {
                        waitForElement(selector, callback);
                    }, 100);
                }
            }

            waitForElement('totalAmount', function (totalAmountElement) {
                totalAmountElement.addEventListener('input', handleTotalAmountInput);
            });
        }

        function handleTotalAmountInput(event) {
            var value = event.target.value;
            var sanitizedValue = '';
            var hasDecimal = false;
            var hasNegative = false;
            var decimalCount = 0;
            var foundFirstDigit = false;

            for (var i = 0; i < value.length; i++) {
                var char = value[i];

                if (char === '-' && !hasNegative && sanitizedValue.length === 0) {
                    hasNegative = true;
                    sanitizedValue += char;
                } else if (char === '.' && !hasDecimal) {
                    hasDecimal = true;
                    sanitizedValue += char;
                } else if (char.match(/[0-9]/)) {
                    if (!foundFirstDigit && char === '0') {
                        continue;
                    }
                    foundFirstDigit = true;
                    if (hasDecimal) {
                        decimalCount++;
                        if (decimalCount <= 2) {
                            sanitizedValue += char;
                        }
                    } else {
                        sanitizedValue += char;
                    }
                }
            }

            event.target.value = sanitizedValue;

            if (sanitizedValue !== '-' && sanitizedValue !== '.' && sanitizedValue !== '-.' && sanitizedValue !== '') {
                event.target.value = numberWithCommas(sanitizedValue);
            }
        }

        function numberWithCommas(x) {
            var parts = x.split(".");

            parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ",");

            if (parts[1] && parts[1].length > 2) {
                parts[1] = parts[1].substring(0, 2);
            }

            return parts.join(".");
        }

        function waitForElement(selector, callback) {
            var element = document.getElementById(selector);
            if (element) {
                callback(element);
            } else {

                $timeout(function () {
                    waitForElement(selector, callback);
                }, 100);
            }
        }

        function handleElementInput(element) {
            element.addEventListener('input', handleTotalAmountInput);

            var initialValue = element.value.replace(/,/g, '');
            if (!isNaN(initialValue) && initialValue.length > 0) {
                var formattedValue = numberWithCommas(initialValue);
                if (formattedValue.indexOf('.') === -1) {
                    formattedValue += '.00';
                }
                element.value = formattedValue;
            }
        }

        $scope.openNgDialogForAmoritzation = function (data) {

            if ($scope.Model.HedgeType === "CashFlow" && $scope.Model.HedgeState === "Dedesignated") {
                $haService
                    .setUrl("HedgeRelationship/GLMapping")
                    .post($scope)
                    .then(function (response) {
                        var defaultGlAccountId = response.data.GlAccountId.toString();
                        var defaultContraAccountId = response.data.GlContraAcctId.toString();
                        InitializeHedgeRelationshipOptionTimeValueAmort(defaultGlAccountId, defaultContraAccountId);
                        OpenAmortizationDialog();
                    });
            }
            else {
                if (typeof data !== "undefined") {
                    if (typeof data.GLAccountID !== "undefined") {
                        data.GLAccountID = data.GLAccountID.toString();
                    }
                    $scope.HedgeRelationshipOptionTimeValueAmort = data;
                }
                else {
                    InitializeHedgeRelationshipOptionTimeValueAmort();
                }
                OpenAmortizationDialog();
            }

        };

        $scope.selectedActionAmortizationChanged = function (action) {

            var gridObj = $("#amortizationDiv").ejGrid("instance");
            var selectedRow = gridObj.selectedRowsIndexes[0];
            var selectedItem = $scope.Model.HedgeRelationshipOptionTimeValueAmorts[selectedRow];
            $scope.selectedRow = selectedRow;

            if (action === 'Download Excel') {

                $scope.Model.SelectedHedgeRelationshipOptionTimeValueAmortID = selectedItem.ID;

                $haService
                    .setUrl('HedgeRelationship/ExportHedgeAmortizatonSchedule')
                    .download($scope, undefined, 'HedgeAmortizatonSchedule', function (response) {

                    });

            } else if (action === 'Delete') {

                if (selectedRow !== undefined && confirm('Are you sure to delete the selected Amortization Schedule?')) {
                    $haService
                        .setUrl('HedgeRelationshipOptionTimeValueAmort')
                        .setId(selectedItem.ID)
                        .destroy($scope)
                        .then(function () {
                            $scope.init(selectedItem.HedgeRelationshipID, function () {
                                jQuery("#tabs-hedgeRelationship").tabs("option", "active", 3);
                            });
                        });
                }
            } else if (action === 'Edit') {

                $scope.HedgeRelationshipOptionTimeValueAmort = selectedItem;

                ngDialog.open({
                    template: 'ha_amortization',
                    scope: $scope,
                    className: 'ngdialog-theme-default ngdialog-theme-custom custom-height-800 amortizationAddEditDialog',
                    title: 'Add/Edit Amortization',
                    showTitleCloseshowClose: true,
                    width: "500px",
                    closeByEscape: false,
                    closeByDocument: false,
                    preCloseCallback: function () {
                        var totalAmountElement = document.getElementById('totalAmount');
                        var intrinsicValueElement = document.getElementById('intrinsicValue');
                        if (totalAmountElement) {
                            totalAmountElement.removeEventListener('input', handleTotalAmountInput);
                        }
                        if (intrinsicValueElement) {
                            intrinsicValueElement.removeEventListener('input', handleTotalAmountInput);
                        }
                        return true;
                    }
                });

                ['totalAmount', 'intrinsicValue'].forEach(function (selector) {
                    waitForElement(selector, handleElementInput);
                });
            }
        };

        function enableDisableOptionAmortDateFields(enable) {
            $("#optionAmortStartDate").ejDatePicker({ enabled: enable });
            $("#optionAmortEndDate").ejDatePicker({ enabled: enable });
        }

        $scope.selectedItemActionAmortizationChanged = function (action) {

            var gridObj = $("#amortizationDiv1").ejGrid("instance");
            var selectedRow = gridObj.selectedRowsIndexes[0];
            var selectedItem = $scope.Model.HedgeRelationshipOptionTimeValues[selectedRow];
            $scope.selectedRow1 = selectedRow;

            if (action === "Download Excel") {
                $scope.Model.SelectedHedgeRelationshipOptionTimeValueAmortID = selectedItem.ID;
                var fileName = selectedItem.OptionTimeValueAmortType === "OptionTimeValue" ? "HedgeOptionTVAmortizatonSchedule" : "HedgeOptionIVAmortizatonSchedule";

                $haService
                    .setUrl("HedgeRelationship/ExportHedgeOptionAmortizationSchedule")
                    .download($scope, undefined, fileName, function (response) {
                        /* eslint no-console: ["error", { allow: ["log"] }] */

                    });
            } else if (action === 'Delete') {
                var fileName = selectedItem.OptionTimeValueAmortType === "OptionTimeValue" ? "Option Time Value Amortizaton Schedule" : "Option Intrinsic Value Amortizaton Schedule";
                if (selectedRow !== undefined && confirm('Are you sure to delete the selected ' + fileName + '?')) {
                    $haService
                        .setUrl('HedgeRelationshipOptionTimeValueAmort')
                        .setId(selectedItem.ID)
                        .destroy($scope)
                        .then(function () {
                            $scope.init(selectedItem.HedgeRelationshipID, function () {
                                jQuery("#tabs-hedgeRelationship").tabs("option", "active", 5);
                            });
                        });
                }
            }
            else if (action === 'Edit') {

                $scope.HedgeRelationshipOptionTimeValueAmort = selectedItem;
                $scope.HedgeRelationshipOptionTimeValueAmort.AmortizeOptionPremimum = $scope.Model.IsAnOptionHedge;
                $scope.HedgeRelationshipOptionTimeValueAmort.HedgeRelationship = $scope.Model;

                var dialogTitle = $scope.HedgeRelationshipOptionTimeValueAmort.OptionTimeValueAmortType === "OptionTimeValue" ?
                    "Option Time Value Amortization" : "Option Intrinsic Value Amortization";

                ngDialog.open({
                    template: 'haOptionTimeValueAmort',
                    scope: $scope,
                    className: 'ngdialog-theme-default ngdialog-theme-custom custom-height-800 amortizationAddEditDialog',
                    title: dialogTitle,
                    showTitleCloseshowClose: true,
                    width: "500px",
                    height: "500px",
                    closeByEscape: false,
                    closeByDocument: false,
                    onOpenCallback: function (e) {
                        enableDisableOptionAmortDateFields($scope.HedgeRelationshipOptionTimeValueAmort.AmortizationMethod !== "Swaplet");
                    },
                    preCloseCallback: function () {
                        var totalAmountElement = document.getElementById('totalAmount');
                        var intrinsicValueElement = document.getElementById('intrinsicValue');;
                        if (totalAmountElement) {
                            totalAmountElement.removeEventListener('input', handleTotalAmountInput);
                        }

                        if (intrinsicValueElement) {
                            intrinsicValueElement.removeEventListener('input', handleTotalAmountInput);
                        }
                        return true;
                    }
                });

                function waitForElement(selector, callback) {
                    var element = document.getElementById(selector);
                    if (element) {
                        callback(element);
                    } else {
                        $timeout(function () {
                            waitForElement(selector, callback);
                        }, 100);
                    }
                }

                waitForElement('totalAmount', function (totalAmountElement) {
                    totalAmountElement.addEventListener('input', handleTotalAmountInput);

                    var initialValue = totalAmountElement.value.replace(/,/g, '');
                    if (!isNaN(initialValue) && initialValue.length > 0) {
                        var formattedValue = numberWithCommas(initialValue);
                        if (formattedValue.indexOf('.') === -1) {
                            formattedValue += '.00';
                        }
                        totalAmountElement.value = formattedValue;
                    }
                });

                waitForElement('intrinsicValue', function (intrinsicValueElement) {
                    intrinsicValueElement.addEventListener('input', handleTotalAmountInput);

                    var initialValue = intrinsicValueElement.value.replace(/,/g, '');
                    if (!isNaN(initialValue) && initialValue.length > 0) {
                        var formattedValue = numberWithCommas(initialValue);
                        if (formattedValue.indexOf('.') === -1) {
                            formattedValue += '.00';
                        }
                        intrinsicValueElement.value = formattedValue;
                    }
                });
            }
        };

        $scope.filterByOptionAmortType = function (selected, index, array) {
            return !($scope.HedgeRelationshipOptionTimeValueAmort.OptionTimeValueAmortType === "OptionTimeValue" && selected.Value === "Swaplet");
        }

        $scope.initAmortizationDiv = function () {
            $timeout(function () {

                if ($scope.Model.HedgeRelationshipOptionTimeValueAmorts === undefined)
                    $scope.Model.HedgeRelationshipOptionTimeValueAmorts = [];

                $scope.Model.HedgeRelationshipOptionTimeValueAmorts.map(function (i) {
                    i.StartDate_ = new Date(i.StartDate);
                    i.FrontRollDate_ = new Date(i.FrontRollDate);
                    i.BackRollDate_ = new Date(i.BackRollDate);
                    i.EndDate_ = new Date(i.EndDate);
                });

                $("#amortizationDiv").ejGrid({
                    dataSource: ej.DataManager($scope.Model.HedgeRelationshipOptionTimeValueAmorts),
                    columns: [
                        { field: "GLAccount.AccountDescription", headerText: "GL Account", textAlign: "left", headerTextAlign: "center", width: 120 },
                        { field: "ContraAccount.AccountDescription", headerText: "Contra", textAlign: "left", headerTextAlign: "center", width: 120 },
                        { field: "StartDate_", headerText: "Start Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "FrontRollDate_", headerText: "Front Roll Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "BackRollDate_", headerText: "Back Roll Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "EndDate_", headerText: "End Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "TotalAmount", headerText: "Total Amount", textAlign: "right", headerTextAlign: "center", format: "{0:C2}", width: 100 },
                        { field: "TotalDCF", headerText: "Total DCF", textAlign: "right", headerTextAlign: "center", format: "{0:N5}", width: 100 },
                        { field: "AmortizationMethodText", headerText: "Method", textAlign: "left", headerTextAlign: "center", width: 120 },
                        {
                            headerText: "Actions",
                            template: "true", templateID: "#actionAmortizationTemplate", isUnbound: true, width: 100
                        }
                    ],
                    enableAltRow: false,
                    allowSorting: true,
                    allowSelection: true,
                    allowTextWrap: true,
                    enableRowHover: true,
                    isResponsive: true,
                    allowSearching: true,
                    selectionSettings: { selectionMode: ["row"] },
                    rowSelecting: function (args) {
                        $scope.initAmortizationScheduleDiv(args.selectedData);
                    },
                    dataBound: function (args) {
                        $compile($(".hedgeActionSelectionAmortization"))($scope);
                    },
                    actionComplete: function (args) {
                        if ($scope.selectedRow === undefined) {
                            $scope.selectedRow = $scope.Model.HedgeRelationshipOptionTimeValueAmorts.length - 1;
                            if ($scope.selectedRow < 0) {
                                $scope.selectedRow = 0;
                            }
                        }

                        this.selectRows([$scope.selectedRow]);
                        $compile($(".hedgeActionSelectionAmortization"))($scope);
                    }
                });

            }, 1);

            $scope.initAmortizationScheduleDiv();
        };

        $scope.initAmortizationDiv1 = function () {
            $timeout(function () {

                if ($scope.Model.HedgeRelationshipOptionTimeValues === undefined) {
                    $scope.Model.HedgeRelationshipOptionTimeValues = [];
                }

                $scope.Model.HedgeRelationshipOptionTimeValues.map(function (i) {
                    i.StartDate_ = new Date(i.StartDate);
                    i.FrontRollDate_ = new Date(i.FrontRollDate);
                    i.BackRollDate_ = new Date(i.BackRollDate);
                    i.EndDate_ = new Date(i.EndDate);
                });

                $("#amortizationDiv1").ejGrid({
                    dataSource: ej.DataManager($scope.Model.HedgeRelationshipOptionTimeValues),
                    columns: [
                        { field: "GLAccount.AccountDescription", headerText: "GL Account", textAlign: "left", headerTextAlign: "center", width: 120 },
                        { field: "ContraAccount.AccountDescription", headerText: "Contra", textAlign: "left", headerTextAlign: "center", width: 120 },
                        { field: "StartDate_", headerText: "Start Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "FrontRollDate_", headerText: "Front Roll Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "BackRollDate_", headerText: "Back Roll Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "EndDate_", headerText: "End Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}", width: 80 },
                        { field: "TotalAmount", headerText: "Total Amount", textAlign: "right", headerTextAlign: "center", format: "{0:C2}", width: 100 },
                        { field: "AmortizationMethodText", headerText: "Method", textAlign: "left", headerTextAlign: "center", width: 120 },
                        { field: "OptionTimeValueAmortTypeText", headerText: "Value Type", textAlign: "left", headerTextAlign: "center", width: 120 },
                        {
                            headerText: "Actions",
                            template: "true", templateID: "#actionAmortizationTemplate1", isUnbound: true, width: 100
                        }
                    ],
                    enableAltRow: false,
                    allowSorting: true,
                    allowSelection: true,
                    allowTextWrap: true,
                    enableRowHover: true,
                    isResponsive: true,
                    allowSearching: true,
                    selectionSettings: { selectionMode: ["row"] },
                    rowSelecting: function (args) {
                        if (args.rowIndex !== args.prevRowIndex) {
                            $scope.initAmortizationScheduleDiv1(args.selectedData);
                        }
                    },
                    dataBound: function (args) {
                        $compile($(".hedgeActionSelectionTest"))($scope);
                    },
                    actionComplete: function (args) {
                        if ($scope.selectedRow1 === undefined) {
                            $scope.selectedRow1 = 0;
                        }

                        this.selectRows([$scope.selectedRow1]);
                        $compile($(".hedgeActionSelectionTest"))($scope);
                    }
                });

            }, 1);

            $scope.initAmortizationScheduleDiv1();
        };

        $scope.OptionTimeValueAmortRollSchedules = [];

        $scope.initAmortizationScheduleDiv = function (data) {

            if (data === undefined && $scope.Model.HedgeRelationshipOptionTimeValueAmorts !== undefined) {
                data = $scope.Model.HedgeRelationshipOptionTimeValueAmorts[0];
            }

            if (data === undefined) {
                data = {
                    OptionTimeValueAmortRollSchedules: []
                };
            }

            $timeout(function () {

                if (data.OptionTimeValueAmortRollSchedules === undefined) {
                    data.OptionTimeValueAmortRollSchedules = [];
                }

                data.OptionTimeValueAmortRollSchedules.map(function (i) {
                    i.StartDate_ = new Date(i.StartDate);
                    i.EndDate_ = new Date(i.EndDate);
                    i.PaymentDate_ = new Date(i.PaymentDate);
                });

                $scope.OptionTimeValueAmortRollSchedules = data.OptionTimeValueAmortRollSchedules;

                $("#amortizationScheduleDiv").ejGrid({
                    dataSource: ej.DataManager(data.OptionTimeValueAmortRollSchedules),
                    columns: [
                        { field: "StartDate_", headerText: "Start Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                        { field: "EndDate_", headerText: "End Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                        { field: "PaymentDate_", headerText: "Payment Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                        { field: "DCF", headerText: "DCF", textAlign: "right", headerTextAlign: "center", format: "{0:N5}" },
                        { field: "Fraction", headerText: "Fraction", textAlign: "right", headerTextAlign: "center", format: "{0:N5}" },
                        { field: "Periodic", headerText: "Periodic", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" }
                    ],
                    locale: "de-DE",
                    allowSelection: true,
                    enableAltRow: false,
                    enableRowHover: true,
                    isResponsive: true,
                    allowSorting: true,
                    allowSearching: true
                });

            }, 1);
        };

        function amortizationScheduleQueryCellInfo(args) {
            var dateFields = ["StartDate_", "EndDate_", "ResetDate_", "PaymentDate_"];
            if (dateFields.indexOf(args.column.field) >= 0 && args.text === "NaN/NaN/0NaN") {
                $(args.cell).text("");
            }
        }

        $scope.initAmortizationScheduleDiv1 = function (data) {
            if (data === undefined && $scope.Model.HedgeRelationshipOptionTimeValues !== undefined) {
                data = $scope.Model.HedgeRelationshipOptionTimeValues[0];
            }

            if (data === undefined) {
                data = {
                    OptionTimeValueAmortRollSchedules: []
                };
            }

            $timeout(function () {

                $scope.SelectedOptionValueType = data.OptionTimeValueAmortType === "OptionTimeValue" ? "Option Time Value Amortizations" : "Option Intrinsic Value Amortizations";

                var optionAmortGridObj = $("#amortizationScheduleDiv1").data("ejGrid");
                if (optionAmortGridObj) {
                    optionAmortGridObj.destroy();
                }

                if (data.AmortizationMethod === "Swaplet" && data.OptionTimeValueAmortType === "OptionIntrinsicValue") {
                    if (typeof data.OptionSwapletAmortizations === "undefined") {
                        data.OptionSwapletAmortizations = [];
                    }

                    data.OptionSwapletAmortizations.map(function (i) {
                        i.StartDate_ = new Date(i.StartDate);
                        i.EndDate_ = new Date(i.EndDate);
                        i.ResetDate_ = new Date(i.ResetDate);
                        i.PaymentDate_ = new Date(i.PaymentDate);
                        i.MonthEnding_ = new Date(i.MonthEnding);
                    });

                    var strikeVisible = true;

                    if ($scope.Model.HedgingItems.length > 0) {
                        strikeVisible = $scope.Model.HedgingItems[$scope.Model.HedgingItems.length - 1].SecurityType === "Collar" ? false : true;
                    }

                    $("#amortizationScheduleDiv1").ejGrid({
                        dataSource: ej.DataManager(data.OptionSwapletAmortizations),
                        columns: [
                            { field: "StartDate_", width: 150, headerText: "Start Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "EndDate_", width: 150, headerText: "End Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "Days", width: 100, headerText: "Days", textAlign: "right", headerTextAlign: "center" },
                            { field: "PaymentDate_", width: 150, headerText: "Payment Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "ResetDate_", width: 150, headerText: "Reset Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "Strike", width: 100, headerText: "Strike", textAlign: "right", headerTextAlign: "center", format: "{0:N3}%", visible: strikeVisible },
                            { field: "DayCountFraction", width: 100, headerText: "DCF", textAlign: "right", headerTextAlign: "center", format: "{0:N5}" },
                            { field: "DiscFactor", width: 120, headerText: "Disc Factor", textAlign: "right", headerTextAlign: "center", format: "{0:N5}" },
                            { field: "Notional", width: 200, headerText: "Notional", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "Rate", width: 130, headerText: "Imp Forward", textAlign: "right", headerTextAlign: "center", format: "{0:N5}%" },
                            { field: "Spread", width: 100, headerText: "Spread", textAlign: "right", headerTextAlign: "center", format: "{0:N2}" },
                            { field: "OptValue", width: 200, headerText: "Option Value", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "OptIntrinsicValue", width: 200, headerText: "Intrinsic Value", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "OptTimeValue", width: 200, headerText: "Time Value", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "MonthEnding_", width: 150, headerText: "Month Ending", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "CycleIncluded", width: 140, headerText: "Cycle Included", textAlign: "right", headerTextAlign: "center" },
                            { field: "CycleExcluded", width: 150, headerText: "Cycle Excluded", textAlign: "right", headerTextAlign: "center" },
                            { field: "CycleIncludedAmount", width: 200, headerText: "Cycle Included $", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "CycleAdjustedAmount", width: 200, headerText: "Cycle Adjusted $", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "OptIntrinsicValueAccrued", width: 200, headerText: "Intrinsic Value Accrued", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" }
                        ],
                        locale: "de-DE",
                        allowSelection: true,
                        enableAltRow: false,
                        enableRowHover: true,
                        isResponsive: true,
                        allowSorting: true,
                        allowSearching: true,
                        allowScrolling: true,
                        scrollSettings: { frozenColumns: 2, width: "100%" },
                        queryCellInfo: amortizationScheduleQueryCellInfo
                    });
                }
                else {
                    if (data.OptionAmortizations === undefined) {
                        data.OptionAmortizations = [];
                    }

                    data.OptionAmortizations.map(function (i) {
                        i.StartDate_ = new Date(i.StartDate);
                        i.EndDate_ = new Date(i.EndDate);
                        i.FixingDate_ = new Date(i.FixingDate);
                        i.PaymentDate_ = new Date(i.PaymentDate);
                    });

                    $("#amortizationScheduleDiv1").ejGrid({
                        dataSource: ej.DataManager(data.OptionAmortizations),
                        columns: [
                            { field: "StartDate_", headerText: "Start Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "EndDate_", headerText: "End Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "FixingDate_", headerText: "Fixing Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "PaymentDate_", headerText: "Amortization Date", textAlign: "left", headerTextAlign: "center", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "Weight", headerText: "Weight", textAlign: "right", headerTextAlign: "center", format: "{0:N2}" },
                            { field: "Amount1", headerText: "Opt 1 Premium", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "Amount2", headerText: "Opt 2 Premium", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" },
                            { field: "TotalAmount", headerText: "Total Amount", textAlign: "right", headerTextAlign: "center", format: "{0:C2}" }
                        ],
                        locale: "de-DE",
                        allowSelection: true,
                        enableAltRow: false,
                        enableRowHover: true,
                        isResponsive: true,
                        allowSorting: true,
                        allowSearching: true
                    });
                }


                if (!$scope.$$phase) {
                    $scope.$apply();
                }
            }, 1);
        };

        $scope.selectedIVAmortizationMethodChanged = function (amortizationMethod) {
            if (amortizationMethod === "Swaplet") {
                $scope.HedgeRelationshipOptionTimeValueAmort.IntrinsicValue = $scope.OptionTimeValueAmortDefaults.IntrinsicValue;
            }
            else {
                $scope.HedgeRelationshipOptionTimeValueAmort.IntrinsicValue = 0;
            }
        }

        $scope.selectedAmortizationMethodChanged = function (amortizationMethod) {
            enableDisableOptionAmortDateFields(amortizationMethod !== "Swaplet");
        }

        $scope.RegressionTestResultsTableInit = function () {

            var batchInputs = [];

            if ($scope.Model.LatestHedgeRegressionBatch !== undefined && $scope.Model.LatestHedgeRegressionBatch !== null && $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults !== undefined) {
                $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults.map(function (v) {
                    batchInputs.push(v);
                });
                var IsIntrinsicValues = ($scope && $scope.Model && ($scope.Model.ProspectiveEffectivenessMethodID === '11' && $scope.Model.RetrospectiveEffectivenessMethodID === '11'));
                batchInputs.map(function (v, k) {
                    v.ObservationIndexView = ($scope.Model.LatestHedgeRegressionBatch.HedgeResultType === 'Periodic'
                        || $scope.Model.LatestHedgeRegressionBatch.HedgeResultType === 'Backload') ? k + 1 : k;
                });

                var hedgedItemHeaderText = "Hedged Item <br/> (DPI " + $scope.Model.LatestHedgeRegressionBatch.HedgedRelationshipItem.ItemID + ")";
                var hedgingItemHeaderText = "Hedging Item <br/> (DPI " + $scope.Model.LatestHedgeRegressionBatch.HedgingRelationshipItem.ItemID + ")";

                if (!$scope.buildRegressionTable) {
                    $timeout(function () {

                        var allZeros = true;
                        batchInputs.map(function (i) {
                            i.ValueDate_ = new Date(i.ValueDate);

                            if (allZeros && i.OptionTimeValue !== 0) {
                                allZeros = false;
                            }
                        });

                        if ($scope.Model.OffMarket === false) {
                            allZeros = true;
                        }

                        var regressionResultDivCols = [
                            { field: "ObservationIndex", headerText: "Obs. <br />Index", width: 90 },
                            { field: "ValueDate_", headerText: "Curve Date", textAlign: "right", headerTextAlign: "right", type: "date", format: "{0:MM/dd/yyyy}" },
                            { field: "HedgedFairValueFormatted", headerText: hedgedItemHeaderText, textAlign: "right", headerTextAlign: "right" },
                            { field: "HedgingFairValueFormatted", headerText: hedgingItemHeaderText, textAlign: "right", headerTextAlign: "right" },
                            { field: "HedgedFairValueChangedFormatted", headerText: hedgedItemHeaderText, textAlign: "right", headerTextAlign: "right" },
                            { field: "HedgingFairValueChangedFormatted", headerText: hedgingItemHeaderText, textAlign: "right", headerTextAlign: "right" },
                            { field: "OptionTimeValueFormatted", headerText: "Time Value", textAlign: "right", headerTextAlign: "right", visible: true },
                            { field: "AdjustedValueFormatted", headerText: "Adjusted Value", textAlign: "right", headerTextAlign: "right", visible: true }
                        ];


                        if (allZeros) {
                            regressionResultDivCols[6].visible = false;
                            regressionResultDivCols[7].visible = false;
                        }

                        $("#regressionResultDiv").ejGrid({
                            dataSource: batchInputs,
                            showStackedHeader: true,
                            stackedHeaderRows: [{
                                stackedHeaderColumns:
                                    [
                                        { headerText: IsIntrinsicValues ? 'Intrinsic Values' : "Fair Values", column: "HedgedFairValueFormatted,HedgingFairValueFormatted" },
                                        { headerText: IsIntrinsicValues ? 'Intrinsic Value Changes' : "Fair Value Changes", column: "HedgedFairValueChangedFormatted, HedgingFairValueChangedFormatted" }
                                    ]
                            }],
                            columns: regressionResultDivCols,
                            rowDataBound: function (args) {
                                var hedgedFairValue = parseFloat(args.rowData.HedgedFairValue);
                                var hedgingFairValue = parseFloat(args.rowData.HedgingFairValue);
                                var hedgedFairValueChanged = parseFloat(args.rowData.HedgedFairValueChanged);
                                var hedgingFairValueChanged = parseFloat(args.rowData.HedgingFairValueChanged);
                                var optionTimeValue = parseFloat(args.rowData.OptionTimeValue);
                                var adjustedValue = parseFloat(args.rowData.AdjustedValue);

                                var hedgedFairValueCol = $(args.row[0].cells[2]);
                                var hedgingFairValueCol = $(args.row[0].cells[3]);
                                var hedgedFairValueChangedCol = $(args.row[0].cells[4]);
                                var hedgingFairValueChangedCol = $(args.row[0].cells[5]);
                                var optionTimeValueCol = $(args.row[0].cells[6]);
                                var adjustValueCol = $(args.row[0].cells[7]);

                                if (hedgedFairValue === 0) {
                                    hedgedFairValueCol.html('-');
                                }

                                if (hedgingFairValue === 0) {
                                    hedgingFairValueCol.html('-');
                                }

                                if (hedgedFairValueChanged === 0) {
                                    hedgedFairValueChangedCol.html('-');
                                }

                                if (hedgingFairValueChanged === 0) {
                                    hedgingFairValueChangedCol.html('-');
                                }

                                if (optionTimeValue === 0) {
                                    optionTimeValueCol.html('-');
                                }

                                if (adjustedValue === 0) {
                                    adjustValueCol.html('-');
                                }

                                hedgedFairValueCol.addClass(hedgedFairValue > 0 ? "green" : "#d84e48");
                                hedgingFairValueCol.addClass(hedgingFairValue > 0 ? "green" : "red");
                                hedgedFairValueChangedCol.addClass(hedgedFairValueChanged > 0 ? "green" : "red");
                                hedgingFairValueChangedCol.addClass(hedgingFairValueChanged > 0 ? "green" : "red");
                                optionTimeValueCol.addClass(optionTimeValue > 0 ? "green" : "red");
                                adjustValueCol.addClass(adjustedValue > 0 ? "green" : "red");

                                if (theme === "dark") {
                                    hedgedFairValueChangedCol.css("background-color", "#333333");
                                    adjustValueCol.css("background-color", "#333333");

                                    if (allZeros) {
                                        hedgingFairValueChangedCol.css("background-color", "#333333");
                                    }
                                }
                                else if (theme === "light") {
                                    hedgedFairValueChangedCol.css("background-color", "#E1F0F9");
                                    adjustValueCol.css("background-color", "#E1F0F9");

                                    if (allZeros) {
                                        hedgingFairValueChangedCol.css("background-color", "#E1F0F9");
                                    }
                                }
                            },
                            enableAltRow: false,
                            allowSorting: true,
                            allowScrolling: true,
                            minWidth: 500,
                            scrollSettings: { height: 485 }
                        });
                    }, 1);
                }
            }


            $scope.selectedItemActionTestChanged = function (action) {

                var obj = $("#allTestsDiv").data("ejGrid");

                if (action === 'Download Excel') {
                    var model = $scope.Model;

                    model.HedgeRegressionForExport = obj.getSelectedRecords()[0].ID;
                    $haService
                        .setUrl('HedgeRegressionBatch/Export/Xlsx')
                        .download($scope, undefined, 'HedgeRegressionBatch');
                }
                else if (action === 'Delete') {
                    var gridObj = $("#allTestsDiv").ejGrid("instance");
                    var selectedRow = gridObj.selectedRowsIndexes[0];
                    var selectedItem = $scope.Model.HedgeRegressionBatches[selectedRow];

                    if (selectedRow !== undefined && confirm('Are you sure to delete the selected test?')) {
                        $haService
                            .setUrl('HedgeRelationship/DeleteBatch')
                            .setId(selectedItem.ID)
                            .post($scope)
                            .then(function (response) {
                                setModelData(response.data);
                            });
                    }
                }
            };


            if ($scope.Model.HedgeRegressionBatches !== undefined) {

                $scope.Model.HedgeRegressionBatches.map(function (i) {
                    i.RunDate_ = new Date(i.RunDate);
                    i.ValueDate_ = new Date(i.ValueDate);
                });


                $("#allTestsDiv").ejGrid({
                    dataSource: ej.DataManager($scope.Model.HedgeRegressionBatches),
                    columns: [
                        { field: "RunDate_", headerText: "Run Date", type: "date", format: "{0:MM/dd/yyyy}", width: 110 },
                        { field: "ValueDate_", headerText: "Curve Date", type: "date", format: "{0:MM/dd/yyyy}", width: 110 },
                        { field: "HedgeResultTypeText", headerText: "Result Type", width: 110 },
                        { field: "HedgedRelationshipItem.ItemID", headerText: "Hedged Item", width: 100 },
                        { field: "HedgingRelationshipItem.ItemID", headerText: "Hedging Item", width: 100 },
                        { field: "RunBy.Person.FullName", headerText: "Run By", width: 120 },
                        { field: "ProspectiveDescription", headerText: "Prospective", width: 120 },
                        { field: "RetrospectiveDescription", headerText: "Retrospective", width: 120 },
                        {
                            headerText: "Actions",
                            template: "true", templateID: "#actionTestTemplate", isUnbound: true, width: 100
                        }
                    ],
                    enableAltRow: false,
                    allowSorting: true,
                    allowSelection: true,
                    allowTextWrap: true,
                    enableRowHover: true,
                    isResponsive: true,
                    allowSearching: true,
                    selectionSettings: { selectionMode: ["row"] },
                    rowSelecting: function (args) {
                        if ($(args.target)[0].cellIndex !== undefined) {
                            $scope.Model.LatestHedgeRegressionBatch = args.data;
                            $scope.$apply();
                            $scope.RegressionTestResultsTableInit();
                        }
                    },
                    dataBound: function (args) {
                        $compile($(".hedgeActionSelectionTest"))($scope);
                    },
                    actionComplete: function (args) {
                        $compile($(".hedgeActionSelectionTest"))($scope);
                    }
                });
            }

            $scope.getRegressionTestResultsGraph();
        };

        $scope.generateSnapshots = function (isInit) {

            if ($scope.Model.HedgeRegressionBatches !== undefined) {
                var slopes = [];
                var r2s = [];

                var batches = $scope.Model.HedgeRegressionBatches.filter(function (v, bi) {
                    if (bi < 8 && (v.HedgeResultType !== 'User' && $scope.Model.HedgeState !== 'Draft') || $scope.Model.HedgeState === 'Draft') {
                        return v;
                    }
                });
                batches.sort(function (a, b) { return (new Date(a.ValueDate) > new Date(b.ValueDate)) ? 1 : ((new Date(b.ValueDate) > new Date(a.ValueDate)) ? -1 : 0); });

                batches.map(function (b) {
                    var vDate1 = false;
                    var vDate2 = false;

                    slopes.map(function (v) {
                        if (!vDate1 && v.x === b.ValueDate) {
                            vDate1 = true;
                        }
                    });

                    r2s.map(function (v) {
                        if (!vDate2 && v.x === b.ValueDate) {
                            vDate2 = true;
                        }
                    });

                    var slope = Number(Math.round(b.Slope + 'e2') + 'e-2');
                    var rSquared = Number(Math.round(b.RSquared + 'e2') + 'e-2');

                    if (!vDate1) { slopes.push({ x: b.ValueDate, y: slope }); }
                    if (!vDate2) { r2s.push({ x: b.ValueDate, y: rSquared }); }
                });

                if (!isInit || isInit === undefined || isInit === null) {
                    var chartObj = $("#divEffSnapshot").data("ejChart");
                    if (chartObj !== undefined && chartObj !== null) { chartObj.redraw(); }
                }
                else if (r2s.length > 0) {
                    $timeout(function () {
                        if (getUserTheme() == 'dark') {
                            $('#divEffSnapshot').ejChart({
                                primaryXAxis:
                                {
                                    labelFormat: 'MM-yy',
                                    valueType: 'datetime',
                                    font: {
                                        color: '#ffffff'
                                    }
                                },
                                primaryYAxis:
                                {
                                    font: {
                                        color: '#ffffff'
                                    }
                                },
                                margin: { left: 10, right: 10, top: 10, bottom: 0 },
                                axes: [{
                                    majorGridLines:
                                    {
                                        visible: false,

                                    },
                                    orientation: 'Vertical',
                                    opposedPosition: true,
                                    axisLine: { visible: false },
                                    rangePadding: 'normal',
                                    name: 'yAxis',
                                    labelFormat: '{value}',
                                    font: {
                                        color: '#ffffff'
                                    }
                                }],
                                series: [{
                                    points: r2s,
                                    fill: "#3b98d4",
                                    name: 'R^2',
                                    type: 'column',
                                    enableAnimation: true,
                                    columnWidth: 0.2,
                                    tooltip:
                                    {
                                        format: "#point.x# <br/> R^2 : #point.y#",
                                        visible: true
                                    }
                                }, {
                                    points: slopes,
                                    fill: "#f79321",
                                    name: 'Slope',
                                    type: 'line',
                                    enableAnimation: true,
                                    yAxisName: 'yAxis',
                                    tooltip:
                                    {
                                        format: "#point.x# <br/> Slope : #point.y#",
                                        visible: true
                                    },
                                    marker: {
                                        shape: 'circle',
                                        size: { height: 8, width: 8 },
                                        visible: true
                                    },
                                    border: { width: 2 }
                                }],
                                load: "loadTheme",
                                isResponsive: true,
                                title: {
                                    text: 'Effectiveness Snapshot',
                                    font: {
                                        color: '#ffffff'
                                    }
                                },
                                legend: {
                                    visible: true,
                                    shape: 'seriesType',
                                    itemPadding: 15,
                                    font: {
                                        color: '#ffffff'
                                    }
                                }
                            });
                        } else {
                            $('#divEffSnapshot').ejChart({
                                primaryXAxis:
                                {
                                    labelFormat: 'MM-yy',
                                    valueType: 'datetime'
                                },
                                margin: { left: 10, right: 10, top: 10, bottom: 0 },
                                axes: [{
                                    majorGridLines:
                                    {
                                        visible: false
                                    },
                                    orientation: 'Vertical',
                                    opposedPosition: true,
                                    axisLine: { visible: false },
                                    rangePadding: 'normal',
                                    name: 'yAxis',
                                    labelFormat: '{value}'
                                }],
                                series: [{
                                    points: r2s,
                                    fill: "#3b98d4",
                                    name: 'R^2',
                                    type: 'column',
                                    enableAnimation: true,
                                    columnWidth: 0.2,
                                    tooltip:
                                    {
                                        format: "#point.x# <br/> R^2 : #point.y#",
                                        visible: true
                                    }
                                }, {
                                    points: slopes,
                                    fill: "#f79321",
                                    name: 'Slope',
                                    type: 'line',
                                    enableAnimation: true,
                                    yAxisName: 'yAxis',
                                    tooltip:
                                    {
                                        format: "#point.x# <br/> Slope : #point.y#",
                                        visible: true
                                    },
                                    marker: {
                                        shape: 'circle',
                                        size: { height: 8, width: 8 },
                                        visible: true
                                    },
                                    border: { width: 2 }
                                }],
                                load: "loadTheme",
                                isResponsive: true,
                                title: { text: 'Effectiveness Snapshot' },
                                legend: {
                                    visible: true,
                                    shape: 'seriesType',
                                    itemPadding: 15
                                }
                            });
                        }

                        var chartObj = $("#divEffSnapshot").data("ejChart");
                        if (chartObj !== undefined && chartObj !== null) { chartObj.redraw(); }
                    }, 1000);
                }
            }
        };

        $scope.getRegressionTestResultsGraph = function () {
            var graphData = [];
            var item = {};
            var x = [];
            var y = [];

            if ($scope.Model.LatestHedgeRegressionBatch !== undefined && $scope.Model.LatestHedgeRegressionBatch !== null && $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults !== undefined) {

                var allZeros = true;
                for (var i = 0; i < $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults.length; i++) {
                    item = $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults[i];
                    if (allZeros && i.item !== 0) {
                        allZeros = false;
                    }
                }

                for (var i = 0; i < $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults.length; i++) {
                    item = $scope.Model.LatestHedgeRegressionBatch.HedgeRegressionBatchResults[i];
                    var xValue = item.HedgedFairValueChanged === "" ? "0" : item.HedgedFairValueChanged;
                    var yValue = allZeros ? item.HedgingFairValueChanged : item.AdjustedValue;
                    yValue = yValue === "" ? "0" : yValue;

                    graphData.push([
                        parseFloat(xValue),
                        parseFloat(yValue)
                    ]);

                    x.push(parseFloat(xValue));
                    y.push(parseFloat(yValue));
                }

                var minxy = (Math.min.apply(null, x)) < (Math.min.apply(null, y)) ? (Math.min.apply(null, x)) : (Math.min.apply(null, y));
                var maxxy = (Math.max.apply(null, x)) > (Math.max.apply(null, y)) ? (Math.max.apply(null, x)) : (Math.max.apply(null, y));

                //Highcharts
                $scope.RegressionTestResultsGraph = new Highcharts.Chart({
                    chart: {
                        renderTo: 'regressionTestResultsGraphContainer',
                        zoomType: 'xy',
                        backgroundColor: '#66666e',
                        borderColor: '#66666e',
                        borderWidth: 2,
                        type: 'scatter',
                        plotBackgroundColor: '#4e4e55',
                        plotBorderWidth: 1,
                        plotBorderColor: "#f1f1f1",
                        spacing: [20, 20, 20, 20]
                    },
                    title: {
                        text: ''
                    },
                    credits: {
                        enabled: true,
                        text: 'Derivative Path Inc',
                        href: "//derivativepath.com",
                        style: {
                            cursor: 'auto'
                        }
                    },
                    legend: {
                        enabled: false
                    },
                    yAxis: {
                        gridLineWidth: 1,
                        gridLineColor: '#f1f1f1',
                        title: {
                            enabled: false
                        },
                        labels: {
                            enabled: true,
                            style: {
                                fontSize: '8px'
                            },
                            formatter: function () {
                                var x = parseFloat(this.value) / 1000;
                                if (this.value > 0) {
                                    return '<span style="color:white;">$' + x + 'K</span>';
                                }
                                else {
                                    return '<span style="color:red;">$(' + Math.abs(x) + ')K</span>';
                                }
                            }
                        },
                        startOnTick: true,
                        endOnTick: true,
                        max: maxxy,
                        min: minxy,
                        plotLines: [{
                            value: 0,
                            width: 1,
                            color: '#f1f1f1'
                        }],
                        lineColor: "yellow",
                        offset: -262,
                        lineWidth: 0
                    },
                    xAxis: {
                        gridLineWidth: 1,
                        gridLineColor: '#f1f1f1',
                        title: {
                            enabled: false
                        },
                        labels: {
                            enabled: true,
                            style: {
                                fontSize: '8px'
                            },
                            formatter: function () {
                                var x = parseFloat(this.value) / 1000;
                                if (this.value > 0) {
                                    return '<span style="color:white;">$' + x + 'K</span>';
                                }
                                else {
                                    return '<span style="color:red;">$(' + Math.abs(x) + ')K</span>';
                                }
                            }
                        },
                        plotLines: [{
                            value: 0,
                            width: 1,
                            color: '#f1f1f1'
                        }],
                        tickWidth: 0,
                        startOnTick: true,
                        endOnTick: true,
                        max: maxxy,
                        min: minxy,
                        lineColor: "yellow",
                        offset: -172,
                        lineWidth: 0
                    },
                    exportButton: {
                        enabled: false
                    },
                    navigation: {
                        buttonOptions: {
                            enabled: false
                        }
                    },
                    series: [
                        {
                            name: 'Fair Value Changes',
                            data: graphData,
                            color: '#87bfe3',
                            marker: {
                                enabled: true,
                                radius: 5
                            }
                        },
                        {
                            type: 'line',
                            name: 'Timeline',
                            marker: { enabled: false },
                            data: (function () {
                                var data = fitData(graphData).data;
                                data.sort(function (a, b) {
                                    return a[0] - b[0];
                                });
                                return data;
                            })()
                        }
                    ]

                });
            }
        };

        $scope.continue = function () {

            $scope.ha_errors = [];

            if ($scope.Model.BankEntityID === '0') {
                $scope.ha_errors.push("Entity is required");
            }

            if ($scope.Model.DesignationDate === undefined || $scope.Model.DesignationDate === '') {
                $scope.ha_errors.push("Designation Date is required");
            }

            if (new Date($scope.Model.DesignationDate.toString()).setHours(0, 0, 0, 0) > new Date().setHours(0, 0, 0, 0)) {
                $scope.ha_errors.push("Designation Date must equal or earlier than the current date");
            }

            setBenchmarkContractualRateExposure();

            checkOptionPremium();
            setDropDownListEffectivenessMethods();
            if ($scope.ha_errors.length === 0) {
                $scope.submit(undefined, function () {
                    var htmlElem = $('#hedgeaccounting_hedgerelationship');
                    htmlElem.html('HR ID ' + $scope.Model.ID.toString());
                    htmlElem.attr('href', '#');
                    $scope.openDetailsTab = true;
                });
            }
        };

        function setHedgeRelationshipOptionTimeValueAmorts() {
            if ($scope.Model.HedgeRelationshipOptionTimeValueAmorts === undefined) {
                $scope.Model.HedgeRelationshipOptionTimeValueAmorts = [];
            }

            if ($scope.Model.HedgeRelationshipOptionTimeValues !== undefined) {
                $scope.Model.HedgeRelationshipOptionTimeValues.map(function (v) {
                    $scope.Model.HedgeRelationshipOptionTimeValueAmorts.push(v);
                });
            }
        }

        $scope.refreshAfterRegress = function (data) {
            setModelData(data);
            jQuery("#tabs-hedgeRelationship").tabs("option", "active", 1);
            var hedgedGrid = $("#HedgedItemDiv").ejGrid("instance");
            var hedgingGrid = $("#HedgingItemDiv").ejGrid("instance");
            hedgedGrid.dataSource($scope.Model.HedgedItems);
            hedgingGrid.dataSource($scope.Model.HedgingItems);

            $scope.RegressionTestResultsTableInit();
        }

        $scope.regress = function () {

            if ($scope.Model.ProspectiveEffectivenessMethodID === undefined || $scope.Model.ProspectiveEffectivenessMethodID === '0')
                $scope.ha_errors.push("Prospective Effectiveness Method is required");
            if ($scope.Model.RetrospectiveEffectivenessMethodID === undefined || $scope.Model.RetrospectiveEffectivenessMethodID === '0')
                $scope.ha_errors.push("Retrospective Effectiveness Method is required");
            if ($scope.Model.HedgedItems.length === 0
                || $scope.Model.HedgingItems.length === 0)
                $scope.ha_errors.push('Hedged and Hedging Items are required to test');
            if ($scope.Model.ReportCurrency === undefined || $scope.Model.ReportCurrency === 'None')
                $scope.ha_errors.push("Report currency is required");

            $scope.setFieldsFromTextBox('detail');

            if ($scope.ha_errors.length === 0) {

                checkAnalyticsStatus(function () {

                    setHedgeRelationshipOptionTimeValueAmorts();

                    $scope.RegressionRunDate = moment();

                    $haService
                        .setUrl('HedgeRelationship/Regress?hedgeResultType=User')
                        .post($scope)
                        .then(function (response) {
                            $scope.refreshAfterRegress(response.data);
                        });
                });

            }
        };

        function checkAnalyticsStatus(callback) {
            $haService
                .setUrl("HedgeRelationship/IsAnalyticsAvailable")
                .get()
                .then(function (response) {
                    var proceed = response.data;
                    if (!response.data) {
                        proceed = confirm("Analytics service is currently unavailable. Are you sure you want to continue?");
                    }
                    if (proceed) {
                        callback();
                    }
                });
        }

        $scope.backload = function () {
            $scope.setFieldsFromTextBox('detail');

            var batch = $scope.Model.HedgeRegressionBatches;
            var existBackLoad = false;

            for (var i = 0; i < batch.length; i++) {

                if (batch[i].HedgeResultType == 'Backload') {
                    existBackLoad = true;
                }
            }

            if (existBackLoad) {
                $scope.ha_errors.push("Backload already created.");
            }
            else {
                $haService
                    .setUrl('HedgeRelationship/Regress?hedgeResultType=Backload')
                    .post($scope)
                    .then(function (response) {
                        setModelData(response.data);
                    });
            }
        };

        function validateOptionHedgeItems(validTradeTypes, linkedItems) {

            if (linkedItems) {
                var valid = linkedItems.filter(function (item) {
                    return validTradeTypes.indexOf(item.SecurityType) >= 0;
                }).length == linkedItems.length;
                return valid;
            }
            return true;
        }

        function checkOptionHedgeItems() {
            if (!$scope.Model.IsAnOptionHedge) {
                return true;
            }

            var hasInvalidHedgedItem = !validateOptionHedgeItems(["CapFloor", "Collar", "Corridor", "Swaption", "SwapWithOption", "Debt"], $scope.Model.HedgedItems);
            var hasInvalidHedgingItem = !validateOptionHedgeItems(["CapFloor", "Collar", "Corridor", "Swaption", "SwapWithOption"], $scope.Model.HedgingItems);
            if (hasInvalidHedgedItem && hasInvalidHedgingItem) {
                $scope.ha_errors.push("When 'Hedge is an Option’ is checked, Trades participating as a Hedged Item or Hedging Item must be one of the following:"
                    + "\n\tHedged Item - Cap/Floor, Collar, Corridor, Swaption, Swap with Option, Debt"
                    + "\n\tHedging Item - Cap/Floor, Collar, Corridor, Swaption, Swap with Option");
            }
            else if (hasInvalidHedgedItem && !hasInvalidHedgingItem) {
                $scope.ha_errors.push("When 'Hedge is an Option’ is checked, Trade participating as a Hedged Item must be one of the following:"
                    + "\n\tCap/Floor, Collar, Corridor, Swaption, Swap with Option, Debt");
            }
            else if (!hasInvalidHedgedItem && hasInvalidHedgingItem) {
                $scope.ha_errors.push("When 'Hedge is an Option’ is checked, Trade participating as a Hedging Item must be one of the following:"
                    + "\n\tCap/Floor, Collar, Corridor, Swaption, Swap with Option");
            }
        }

        $scope.submit = function (final, callback, caller) {

            $scope.ha_errors = [];
            $scope.relationshipUserMessage = "";

            if ($scope.Model.ProspectiveEffectivenessMethodID === '0')
                delete $scope.Model.ProspectiveEffectivenessMethodID;

            if ($scope.Model.RetrospectiveEffectivenessMethodID === '0')
                delete $scope.Model.RetrospectiveEffectivenessMethodID;

            if ($scope.Model.IsAnOptionHedge) {
                $scope.Model.OffMarket = false;
            }

            var needToConfirm = false;

            if ($scope.Model.DedesignationDate !== undefined && $scope.Model.DedesignationDate !== null) {
                var designationDate = new Date($scope.Model.DesignationDate.toString());
                var dedesignationDate = new Date($scope.Model.DedesignationDate.toString());
                if (dedesignationDate <= designationDate) {
                    $scope.ha_errors.push("Dedesignation Date must be later than Designation Date");
                }
                else if (dedesignationDate <= designationDate.setMonth(designationDate.getMonth() + 3)) {
                    needToConfirm = true;
                }
            }

            if (new Date($scope.Model.DesignationDate.toString()).setHours(0, 0, 0, 0) > new Date().setHours(0, 0, 0, 0)) {
                $scope.ha_errors.push("Designation Date must equal or earlier than the current date");
            }

            if ($scope.Model.HedgeType === 'FairValue' && !$scope.Model.Shortcut && $scope.Model.QualitativeAssessment) {
                $scope.ha_errors.push("Qualitative Assessment cannot be selected for a Fair Value Long Haul Hedge Relationship.");
            }

            if ($scope.Model.HedgeType === 'FairValue' && $scope.Model.Shortcut && $scope.Model.PortfolioLayerMethod) {
                $scope.ha_errors.push("Portfolio Layer Method cannot be selected for a Fair Value Shortcut Hedge Relationship.");
            }

            if ($scope.Model.HedgingInstrumentStructure === 'SingleInstrument' && $scope.Model.HedgingItems && $scope.Model.HedgingItems.length > 1) {
                $scope.ha_errors.push("There is more than one Hedging Instrument participating in the Hedge Relationship.  Either update the Hedging Instrument Structure or remove additional Hedging Instruments.");
            }

            if ($scope.Model.HedgeType === 'FairValue' || $scope.Model.HedgeType === 'NetInvestment') {
                $scope.Model.PreIssuanceHedge = false;
            }

            if ($scope.Model.HedgeType === 'CashFlow' || $scope.Model.HedgeType === 'NetInvestment') {
                $scope.Model.PortfolioLayerMethod = false;
            }

            setBenchmarkContractualRateExposure();

            checkOptionPremium();
            checkOptionHedgeItems();

            $scope.setFieldsFromTextBox(!$scope.openDetailsTab ? 'init' : 'detail');

            if ($scope.ha_errors.length === 0) {
                if ($scope.onActionChangeValue !== "Workflow") {
                    $scope.Model.ActionText = $scope.onActionChangeValue;
                }

                if (!needToConfirm ||
                    (needToConfirm && confirm('Dedesignation date should be 3 months after designation date. Are you sure you want to continue?'))) {
                    $haService
                        .setUrl('HedgeRelationship')
                        .post($scope)
                        .then(function (response) {
                            $scope.relationshipUserMessage = "Success! The hedge relationship was successfully saved.";
                            setModelData(response.data);

                            if (caller === "PreviewInceptionPackage" || caller === "InitiateDesignation" || caller === "InitiateReDesignation") {
                                callback();
                                return;
                            }

                            if (final) {
                                $scope.cancel();
                            }
                            else {
                                setTimeout(function () {
                                    $scope.relationshipUserMessage = '';
                                    $scope.$apply();
                                }, 3000);
                            }

                            $timeout(function () {
                                usermessage();

                                if (callback !== undefined) {
                                    callback();
                                }
                            }, 0);
                        });
                }
            }


        };

        $scope.checkDocumentTemplateKeywords = function (preview) {
            $haService
                .setUrl("HedgeRelationship/CheckDocumentTemplateKeywords/" + id)
                .get()
                .then(function (response) {

                    $timeout(function () {
                        $scope.generatePackage(preview, function () {
                            if (response.data.HasEmptyKeyword) {
                                $scope.ha_errors.push("There are Smart Tags defined in the Hedge Documentation that cannot be replaced with a value. " +
                                    "Within the Hedge Memorandum, the Smart Tag(s) will be replaced with ‘_________________’.");
                            }
                        });

                    }, 1000);
                });
        }

        $scope.generateInceptionPackage = function (preview) {
            $scope.ha_errors = [];
            if (preview === undefined) preview = true;
            $haService
                .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
                .get()
                .then(function (response) {
                    if (response.data) {
                        $scope.submit(undefined, function () {
                            $scope.init(id, function () {
                                $timeout(function () {
                                    $scope.checkDocumentTemplateKeywords(preview);
                                }, 1000);
                            });
                        }, "PreviewInceptionPackage");
                    }
                    else {
                        $scope.generatePackage(preview);
                    }
                });
        };

        $scope.generatePackage = function (preview, callback) {
            $scope.setFieldsFromTextBox('detail');

            $("#loading").show();

            preview = preview ? 'True' : 'False';

            setHedgeRelationshipOptionTimeValueAmorts();

            $haService
                .setUrl('HedgeRelationship/GenerateInceptionPackage?preview=' + preview)
                .download($scope, undefined, 'InceptionPackage', callback);
        };

        $scope.cancel = function () {
            $scope.init(id);
        };

        $scope.openLinkExistingTrade = function (type) {
            $scope.HedgeRelationshipItemType = type;
            var title = type === "HedgingItem" ? "Hedging Item" : "Hedged Item";
            title = title + ": Select Existing Trade";
            ngDialog.open({
                template: 'firstDialog',
                controller: 'linkTradeCtrl',
                controllerAs: 'vm',
                scope: $scope,
                className: 'ngdialog-theme-default ngdialog-theme-custom linkingTradesDialog fxstyleddialog',
                title: title,
                showTitleCloseshowClose: true,
                width: "100%",
                height: "100%",
                closeByEscape: false,
                closeByDocument: false
            });
        };

        $scope.openNewTrade = function (type, itemType) {
            var url = "";
            var oppId = "0";
            var entityId = $scope.Model.BankEntityID !== "0" ? $scope.Model.BankEntityID : $scope.Model.ClientID;
            var title = "";
            var securityType = "";

            $scope.HedgeRelationshipItemType = itemType;

            if (type === "callabledebt") {
                url = "/CallableDebt/ShowAddCallableDebt?id=" + entityId + "&oppId=" + oppId;
                title = "Callable Debt";
                securityType = "CallableDebt";
            }
            else if (type === "cancelable") {
                url = "/Cancelable/ShowAddCancelable?id=" + entityId + "&oppId=" + oppId;
                title = "Cancelable";
                securityType = "Cancelable";
            }
            else if (type === "cap") {
                url = "/CapFloor/ShowAddCapFloor?id=" + entityId + "&oppId=" + oppId;
                title = "Cap Floor";
                securityType = "CapFloor";
            }
            else if (type === "collar") {
                url = "/Collar/Add?cId=" + entityId + "&returnPartial=true";
                title = "Collar";
                securityType = "Collar";
            }
            else if (type === "debt") {
                url = "/Debt/AddDebt?cId=" + entityId + "&returnPartial=true";
                title = "Debt";
                securityType = "Debt";
            }
            else if (type === "debtoption") {
                url = "/DebtOption/ShowAddDebtOption?id=" + entityId + "&oppId=" + oppId;
                title = "Debt Option";
                securityType = "DebtOption";
            }
            else if (type === "swap") {
                url = "/Swap/ShowAddSwap?id=" + entityId + "&oppId=" + oppId;
                title = "Swap";
                securityType = "Swap";
            }
            else if (type === "swapwithoption") {
                url = "/SwapEmbeddedOption/Add?cId=" + entityId + "&returnPartial=true";
                title = "Swap With Option";
                securityType = "SwapWithOption";
            }
            else if (type === "swaption") {
                url = "/Swaption/ShowAddSwaption?id=" + entityId + "&oppId=" + oppId;
                title = "Swaption";
                securityType = "Swaption";
            }
            else if (type === "corridor") {
                url = "/Corridor/Add?cId=" + entityId + "&returnPartial=true";
                title = "Corridor";
                securityType = "Corridor";
            }
            else if (type === "fxforward") {
                url = "/FxSingle/Add?cId=" + entityId + "&type=11&returnPartial=true";
                title = "FX Forward";
                securityType = "FxForward";
            }

            $scope.openNgDialogForTrade(url, title, securityType);
        };

        $scope.openExistingTrade = function (id, itemType, securityType) {
            var url = '/Trade/EditTrade?id=' + id + '&returnPartial=true';

            $scope.HedgeRelationshipItemType = itemType;
            $scope.openNgDialogForTrade(url, null, securityType);
        };

        $scope.openNgDialogForTrade = function (url, title, securityType) {
            var dialogTitle = "Trade Information";
            if (title) {
                dialogTitle = dialogTitle + " - " + title;
            }
            ngDialog.open({
                template: url,
                className: 'ngdialog-theme-default ngdialog-theme-custom fxstyleddialog',
                appendTo: "#tradeInformationDialogBody",
                name: "tradeInformationDialog",
                closeByEscape: false,
                closeByDocument: false,
                title: dialogTitle,
                width: "100%",
                height: "100%",
                showTitleCloseshowClose: true,
                cache: false,
                preCloseCallback: function (data) {
                    var modelScope = $(this).scope();
                    var parentScope = modelScope.$parent.$$childHead;
                    var model = null;
                    if (securityType === "FxForward") {
                        model = $(this).find(".ngDialogContentRow div[id='editfxsingle_container']").scope();
                        if (!model) {
                            model = $(this).find(".ngDialogContentRow div[id='addfxsingle_container']").scope();
                        }
                    }
                    else {
                        model = $(this).find(".ngDialogContentRow div:first-child").scope();
                    }
                    if (model !== null) {
                        model = model.Model;
                        if (model != undefined) {
                            if (model.IsModelDirty !== undefined) {
                                if (model.IsModelDirty === "True" || model.IsModelDirty) {
                                    var nestedConfirmDialog = ngDialog.openConfirm({
                                        template: '\
                                    <div><h4>Are you sure you want to leave this page?</h4><p>You have unsaved changes on this page.</p>\
                                    <div class="ngdialog-buttons">\
                                        <button type="button" class="btn btn-info" ng-click="closeThisDialog(0)">Stay on Page</button>\
                                        <button type="button" class="btn btn-info" ng-click="confirm(1)">Leave Page</button>\
                                    </div></div>',
                                        plain: true,
                                        showClose: false,
                                        width: "400px",
                                        height: "auto",
                                        className: 'ngdialog-theme-default ngdialog-theme-custom dirtyformcheckingngdialog'
                                    });
                                    if (nestedConfirmDialog) {
                                        parentScope.linkExistingTrade(parentScope, model.Id.toString(), model.DpiClient.Id.toString());
                                    }
                                    return nestedConfirmDialog;
                                }
                                else {
                                    parentScope.linkExistingTrade(parentScope, model.Id.toString(), model.DpiClient.Id.toString());
                                }
                            }
                        }
                    }
                    jQuery("#tradeInformationDialogBody").addClass("invisible");
                    return true;
                },
                onOpenCallback: function (e) {
                    if (securityType === "FxForward") {
                        var promise = $interval(function () {
                            var securityTypeDropdownButton = jQuery("[data-id=fxsecurityType]");
                            if (securityTypeDropdownButton.length === 1) {
                                securityTypeDropdownButton.attr("disabled", "disabled");
                                securityTypeDropdownButton.addClass("disabled");
                                $interval.cancel(promise);
                            }
                        }, 200, 300);
                    }
                    else {
                        jQuery(this).find(".ngdialog-content .ngDialogContentRow").attr("id", "tradesAppContainer");
                    }
                }
            });
        };

        $scope.$watch('Model.ClientID', function (new_, old_) {

            if (new_ !== undefined && new_ !== old_) {

                if (new_ > 0) {
                    $haService
                        .setUrl('ClientConfig')
                        .setId(new_)
                        .get()
                        .then(function (response) {
                            var config = response.data;

                            if (config !== null && config !== undefined) {
                                $scope.Model.ClientID = config.ID.toString();

                                if ($scope.Model.HedgeRiskType === 'None')
                                    $scope.Model.HedgeRiskType = config.HedgeRiskType;

                                if ($scope.Model.HedgeType === 'None')
                                    $scope.Model.HedgeType = config.HedgeType;

                                if ($scope.Model.HedgedItemType === 'None')
                                    $scope.Model.HedgedItemType = config.HedgedItemType;

                                if ($scope.Model.HedgedItemTypeDesc === null || $scope.Model.HedgedItemTypeDesc === undefined)
                                    $scope.Model.HedgedItemTypeDesc = config.HedgedItemTypeDesc;

                                if ($scope.Model.ProspectiveEffectivenessMethodID === undefined || $scope.Model.ProspectiveEffectivenessMethodID === '0')
                                    $scope.Model.ProspectiveEffectivenessMethodID = config.ProspectiveEffectivenessMethodID.toString();

                                if ($scope.Model.RetrospectiveEffectivenessMethodID === undefined || $scope.Model.RetrospectiveEffectivenessMethodID === '0')
                                    $scope.Model.RetrospectiveEffectivenessMethodID = config.RetrospectiveEffectivenessMethodID.toString();

                                if ($scope.Model.Observation === 0 || $scope.Model.Observation === undefined)
                                    $scope.Model.Observation = config.Observation;

                                if ($scope.Model.ReportingFrequency === 'None')
                                    $scope.Model.ReportingFrequency = config.ReportingFrequency;

                                if ($scope.Model.Objective === null || $scope.Model.Objective === undefined || $scope.Model.Objective === '')
                                    $scope.Model.Objective = config.Objective;

                                if ($scope.Model.ReportCurrency === null || $scope.Model.ReportCurrency === undefined)
                                    $scope.Model.ReportCurrency = config.ReportCurrency;

                                if ($scope.Model.EOM === null || $scope.Model.EOM === undefined)
                                    $scope.Model.EOM = config.EOM;

                                jQuery("input").trigger("blur");

                                $scope.setTextBoxFromFields();
                                $scope.Model.IsNewHedgeDocumentTemplate = $scope.Model.Objective === undefined || $scope.Model.Objective && $scope.Model.Objective.match(/^(?:<\/?p>|<\/?br\s*\/?>|<\/?div>|<\/?span>|\s|&nbsp;)*$/);
                            }

                            retreiveGLAccounts($scope);
                        });
                }


            }
        });

        $scope.checkboxStraightlineClickEvent = function (e) {
            var value = $(e.currentTarget).hasClass("fa-square-o");
            var type = $(e.currentTarget).attr("data-type");
            type = type.replace("HedgeRelationshipOptionTimeValueAmort.", "");
            $scope.HedgeRelationshipOptionTimeValueAmort[type] = value;
        };

        $scope.checkboxRedesignationClickEvent = function (e) {
            var value = $(e.currentTarget).hasClass("fa-square-o");
            var type = $(e.currentTarget).attr("data-type");
            type = type.replace("Model.", "");
            $scope.Model[type] = value;
        };

        $scope.isRedesignationValid = function () {
            var valid = $scope.Model.Payment && $scope.Model.Payment !== 0;
            valid = valid && moment($scope.Model.RedesignationDate, 'M/D/YYYY', true).isValid();

            var startDate = moment($scope.Model.TimeValuesStartDate, 'M/D/YYYY', true);
            var endDate = moment($scope.Model.TimeValuesEndDate, 'M/D/YYYY', true);
            valid = valid && startDate.isValid();
            valid = valid && endDate.isValid();
            valid = valid && startDate < endDate;

            valid = valid && $scope.Model.PayBusDayConv !== "";
            valid = valid && $scope.Model.PaymentFrequency !== "";
            valid = valid && $scope.Model.DayCountConv !== "";

            return valid;
        };

        $scope.checkboxClickEvent = function (e, model) {

            var value = $(e.currentTarget).hasClass("fa-square-o");
            var type = $(e.currentTarget).attr("data-type");

            if ($scope.Model.HedgeState === "Draft" || $scope.checkUserRole("24")) {
                model = model !== undefined ? model : "Model";
                type = type.replace(model + ".", "");

                if (type === "ExcludeIntrinsicValue") {
                    if (!$scope.Model.IsAnOptionHedge) {
                        value = false;
                    }
                }
                if (type === "IsAnOptionHedge" && $scope.Model.OffMarket) {
                    value = false;
                }
                if (type === "OffMarket" && $scope.Model.IsAnOptionHedge) {
                    value = false;
                }

                $scope[model][type] = value;
            }
        };

        $scope.toggleRegression = function (isCumulative) {
            if ($scope.Model.HedgeState === 'Draft') {
                $scope.Model.CumulativeChanges = isCumulative;
                $scope.Model.PeriodicChanges = !$scope.Model.CumulativeChanges;
            }
        };

        $scope.onChangeTabActionValue = function (value) {
            if (value === "Amortization") {
                $scope.openNgDialogForAmoritzation();
            }
            else if (value === "Option Amortization") {
                $scope.openOptionTimeValueAmortDialog();
            }
        };

        designate = function (callback) {
            $scope.ha_errors = [];
            if ($scope.Model.HedgedItems.length === 0
                || $scope.Model.HedgingItems.length === 0)
                $scope.ha_errors.push("Hedged and Hedging Items are required to generate inception package");
            if ($scope.Model.ReportCurrency === undefined || $scope.Model.ReportCurrency === "None")
                $scope.ha_errors.push("Report currency is required");
            if ($scope.Model.ProspectiveEffectivenessMethodID === undefined || $scope.Model.ProspectiveEffectivenessMethodID === "0")
                $scope.ha_errors.push("Prospective Effectiveness Method is required");
            if ($scope.Model.RetrospectiveEffectivenessMethodID === undefined || $scope.Model.RetrospectiveEffectivenessMethodID === "0")
                $scope.ha_errors.push("Retrospective Effectiveness Method is required");

            for (var i = 0; i < $scope.Model.HedgedItems.length; i++) {
                if ($scope.Model.HedgedItems[i].ItemStatus != "HA") {
                    var msg = "Hedged items must be in HA status.";
                    if (!$scope.ha_errors.includes(msg)) {
                        $scope.ha_errors.push(msg);
                    }
                    else {
                        break;
                    }
                }
            }

            for (var i = 0; i < $scope.Model.HedgingItems.length; i++) {
                if ($scope.Model.HedgingItems[i].ItemStatus != "Validated") {
                    var msg = "Hedging items must be in validated status.";
                    if (!$scope.ha_errors.includes(msg)) {
                        $scope.ha_errors.push(msg);
                    }
                    else {
                        break;
                    }
                }
            }

            if ($scope.Model.DedesignationDate !== undefined && $scope.Model.DedesignationDate !== null
                && new Date($scope.Model.DedesignationDate.toString()) <= new Date($scope.Model.DesignationDate.toString())) {
                $scope.ha_errors.push("Dedesignation Date must be later than Designation Date");
            }
            if ($scope.Model.HedgeType === undefined || $scope.Model.HedgeType === 'None')
                $scope.ha_errors.push("Hedge Type must be specified");
            else if ($scope.Model.HedgeType === 'FairValue') {
                if ($scope.Model.FairValueMethod === undefined || $scope.Model.FairValueMethod === 'None') {
                    $scope.ha_errors.push("Fair Value Method must be specified");
                }
                if ($scope.Model.Benchmark === undefined || $scope.Model.Benchmark === 'None') {
                    $scope.ha_errors.push("Benchmark index must be specified");
                }
            } else if ($scope.Model.HedgeType === "CashFlow" && $scope.Model.HedgeRiskType !== "ForeignExchange" && (typeof $scope.Model.Benchmark === "undefined" || $scope.Model.Benchmark === "None")) {
                $scope.ha_errors.push("Contractual Rate must be specified");
            }

            if ($scope.Model.HedgedItemType === undefined || $scope.Model.HedgedItemType === 'None')
                $scope.ha_errors.push("Hedged Item Type must be specified");
            if ($scope.Model.AssetLiability === undefined || $scope.Model.AssetLiability === 'None')
                $scope.ha_errors.push("Hedged Item must be specified");

            if (new Date($scope.Model.DesignationDate.toString()).setHours(0, 0, 0, 0) > new Date().setHours(0, 0, 0, 0)) {
                $scope.ha_errors.push("Designation Date must equal or earlier than the current date");
            }

            if (
                $scope.Model.HedgeType === "CashFlow" &&
                $scope.Model.OffMarket === true &&
                !$scope.Model.HedgeRelationshipOptionTimeValueAmorts.some(a => a.OptionTimeValueAmortType === "Amortization")
            ) {
                $scope.ha_errors.push("An Amortization Schedule is required for an Off-Market Hedge Relationship.");
            }


            checkOptionHedgeItems();

            $scope.setFieldsFromTextBox('detail');

            if ($scope.ha_errors.length === 0) {
                checkAnalyticsStatus(function () {
                    setHedgeRelationshipOptionTimeValueAmorts();

                    $haService
                        .setUrl('HedgeRelationship/Regress?hedgeResultType=Inception')
                        .post($scope)
                        .then(function () {
                            $scope.init(id, function () {
                                $timeout(function () {

                                    callback();

                                }, 1000);
                            });
                        });
                });
            }
        }

        checkDocumentTemplateKeywordsOnDesignated = function () {
            $scope.checkDocumentTemplateKeywords(false);
        }

        initiateDesignation = function () {
            $scope.ha_errors = [];
            $haService
                .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
                .get()
                .then(function (response) {
                    if (response.data) {
                        $scope.submit(undefined, function () {
                            $scope.init(id, function () {
                                $timeout(function () {
                                    designate(checkDocumentTemplateKeywordsOnDesignated);
                                }, 1000);
                            });
                        }, "InitiateDesignation");
                    }
                    else {
                        designate(function () {
                            $scope.generatePackage(false);
                        });
                    }
                });
        }

        reDesignate = function (isDocTemplateFound) {
            checkAnalyticsStatus(function () {
                $haService
                    .setUrl('HedgeRelationship/Redesignate/' + id)
                    .get()
                    .then(function (response) {
                        $scope.Model.RedesignationDate = moment(response.data.RedesignationDate).format('M/D/YYYY');
                        $scope.Model.TimeValuesStartDate = moment(response.data.TimeValuesStartDate).format('M/D/YYYY');
                        $scope.Model.TimeValuesEndDate = moment(response.data.TimeValuesEndDate).format('M/D/YYYY');
                        $scope.Model.Payment = 0;
                        $scope.Model.DayCountConv = response.data.DayCountConv;
                        $scope.Model.PayBusDayConv = response.data.PayBusDayConv;
                        $scope.Model.PaymentFrequency = response.data.PaymentFrequency;
                        $scope.Model.AdjustedDates = response.data.AdjustedDates;
                        $scope.Model.MarkAsAcquisition = response.data.MarkAsAcquisition;
                        $scope.Model.IsDocTemplateFound = isDocTemplateFound;

                        ngDialog.open({
                            template: 'redesignateDialog',
                            controller: 'reDesignateCtrl',
                            scope: $scope,
                            className: 'ngdialog-theme-default ngdialog-theme-custom',
                            title: 'Re-Designation Workflow',
                            showTitleCloseshowClose: true
                        });
                    });
            });
        }

        initiateReDesignation = function () {
            $scope.ha_errors = [];
            $haService
                .setUrl("HedgeRelationship/FindDocumentTemplate/" + id)
                .get()
                .then(function (response) {
                    if (response.data) {
                        $scope.submit(undefined, function () {
                            $scope.init(id, function () {
                                $timeout(function () {
                                    reDesignate(true);
                                }, 1000);
                            });
                        }, "InitiateReDesignation");
                    }
                    else {
                        reDesignate(false);
                    }
                });
        }

        $scope.onChangeActionValue = function (value) {

            $scope.onActionChangeValue = value;

            if (value === "De-Designate") {

                $scope.DedesignateUserMessage = '';
                $scope.DedesignateDisabled = true;
                $scope.Model.DedesignationReason = 0;
                $scope.Model.Termination = false;
                $scope.Model.Ineffectiveness = false;
                $scope.Model.TimeValuesStartDate = moment().format('M/D/YYYY');
                $scope.Model.TimeValuesEndDate = moment().format('M/D/YYYY');
                $scope.Model.FullCashPayment = true;
                $scope.Model.PartialCashPayment = false;
                $scope.Model.NoCashPayment = false;
                $scope.Model.HedgedExposureNotExist = false;

                var dedesignationDate = moment().format("M/D/YYYY");
                $scope.Model.DeDesignation = {
                    DedesignationDate: dedesignationDate,
                    Payment: 0,
                    Accrual: 0,
                    BasisAdjustment: 0,
                    BasisAdjustmentBalance: 0,
                    CashPaymentType: 0,
                    HedgedExposureExist: true
                };

                if ($scope.Model.HedgingItems.length > 0) {
                    var hedgingItem = $scope.Model.HedgingItems[$scope.Model.HedgingItems.length - 1];

                    var url_ = "";
                    if (hedgingItem.SecurityType === "Swap"
                        || hedgingItem.SecurityType === "CapFloor"
                        || hedgingItem.SecurityType === "Debt"
                        || hedgingItem.SecurityType === "Swaption") {
                        url_ = hedgingItem.SecurityType + "Summary";
                    }
                    else if (hedgingItem.SecurityType === "Cancelable"
                        || hedgingItem.SecurityType === "Collar"
                        || hedgingItem.SecurityType === "DebtOption"
                        || hedgingItem.SecurityType === "CallableDebt"
                        || hedgingItem.SecurityType === "Corridor") {
                        url_ = hedgingItem.SecurityType;
                    }
                    else if (hedgingItem.SecurityType === "SwapWithOption") {
                        url_ = "SwapEmbeddedOption";
                    }

                    if (url_ !== "") {

                        $haService
                            .setUrl("Trade/GetTerminationDate/" + hedgingItem.ItemID)
                            .get()
                            .then(function (response) {
                                if (response.data !== null) {
                                    var terminationDate = moment(response.data).format("M/D/YYYY");

                                    var userAction = "";
                                    if (hedgingItem.SecurityType === "Collar") {
                                        userAction = "&userAction=price_collar";
                                    }

                                    url_ = "/" + url_ + "/Price?id=" + hedgingItem.ItemID + "&valueDate=" + terminationDate + "&instance=Last&discCurve=OIS" + userAction;

                                    $http({
                                        method: "GET",
                                        url: url_,
                                        cache: false
                                    }).then(function (response) {
                                        $scope.Model.DeDesignation.Accrual = jQuery(jQuery.parseHTML(response.data.pricePV)).find("input[name='Accrued']").val();

                                        ngDialog.open({
                                            template: "dedesignateDialog",
                                            controller: "deDesignateCtrl",
                                            scope: $scope,
                                            className: "ngdialog-theme-default ngdialog-theme-custom",
                                            title: "De-Designation Workflow",
                                            showTitleCloseshowClose: true
                                        });

                                    });
                                }
                                else {
                                    ngDialog.open({
                                        template: "dedesignateDialog",
                                        controller: "deDesignateCtrl",
                                        scope: $scope,
                                        className: "ngdialog-theme-default ngdialog-theme-custom",
                                        title: "De-Designation Workflow",
                                        showTitleCloseshowClose: true
                                    });
                                }
                            });
                    }
                }
            }
            else if (value === "Designate") {
                initiateDesignation();
            }
            else if (value === "Redraft") {
                var gridObj = $("#amortizationDiv1").ejGrid("instance");
                var selectedRow = gridObj.selectedRowsIndexes[0];
                var selectedItem = $scope.Model.HedgeRelationshipOptionTimeValues[selectedRow];

                if (selectedItem !== undefined) {
                    $scope.selectedRow1 = selectedRow;
                    $haService
                        .setUrl('HedgeRelationshipOptionTimeValueAmort')
                        .setId(selectedItem.ID)
                        .destroy($scope.selectedItem)
                        .then(function () {
                            $scope.init(selectedItem.HedgeRelationshipID);
                            $haService
                                .setUrl('HedgeRelationship/Redraft')
                                .post($scope)
                                .then(function (response) {
                                    $scope.init(id);
                                });
                        });
                }
                else {
                    $haService
                        .setUrl('HedgeRelationship/Redraft')
                        .post($scope)
                        .then(function (response) {
                            $scope.init(id);
                        });
                }
            }
            else if (value === "Re-Designate") {

                initiateReDesignation();
            }
        };

        $scope.retreiveDatetoCorrectFormat = function (value) {
            if (value.substring(0, 6) === "/Date(") {
                var dt = new Date(parseInt(value.substring(6, value.length - 2)));
                dt = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
                var dtString = (dt.getMonth() + 1) + "/" + dt.getDate() + "/" + dt.getFullYear();
                return dtString;
            }
            return value;
        };

        $scope.linkExistingTrade = function (scope, tradeId, clientID, ngDialog) {

            if (parseInt(tradeId) > 0) {

                $haService
                    .setUrl('Trade/GetForHedging/' + tradeId + '/' + clientID)
                    .get()
                    .then(function (response) {
                        var trade = response.data;

                        var legs = [];
                        trade.HedgeRelationshipItemLegs.map(function (v) {
                            legs.push({
                                CmpdgPeriod: v.CmpdgPeriod == undefined ? null : v.CmpdgPeriod,
                                FixedRate: v.FixedRate,
                                FixFlt: v.FixFlt,
                                Index: v.Index,
                                IdxPct: v.IdxPct,
                                ResetFreq: v.ResetFreq == undefined ? null : v.ResetFreq,
                                IndexTenor: v.IndexTenor == undefined ? null : v.IndexTenor,
                                PayRec: v.PayRec
                            });
                        });

                        var item = {
                            "$id": trade.ItemID,
                            HedgeRelationshipItemType: scope.HedgeRelationshipItemType,
                            TradeDate: trade.TradeDate == undefined ? null : app.retreiveDatetoCorrectFormat(new Date(trade.TradeDate)),
                            Description: trade.Description,
                            EffectiveDate: trade.EffectiveDate == undefined ? null : app.retreiveDatetoCorrectFormat(new Date(trade.EffectiveDate)),
                            ItemID: trade.ItemID,
                            MaturityDate: trade.MaturityDate == undefined ? null : app.retreiveDatetoCorrectFormat(new Date(trade.MaturityDate)),
                            Notional: trade.Notional,
                            Rate: trade.Rate,
                            SecurityType: trade.SecurityType,
                            SecurityTypeText: trade.SecurityTypeText,
                            Spread: trade.Spread,
                            ItemStatus: trade.ItemStatus,
                            ItemStatusText: trade.ItemStatusText,
                            BuySell: trade.BuySell == undefined ? null : trade.BuySell,
                            PutCall: trade.PutCall == undefined ? null : trade.PutCall,
                            Strike: trade.Strike == undefined ? null : trade.Strike,
                            CounterPartyId: trade.CounterPartyId == undefined ? null : trade.CounterPartyId,
                            HedgeRelationshipItemLegs: legs
                        };

                        if (trade.SecurityType === "FxForward") {
                            item.EffectiveDate = item.TradeDate;
                        }

                        var existingHedgedItem = scope.Model.HedgedItems.filter(function (obj) {
                            if (obj.ItemID == item.ItemID)
                                return true;
                        });

                        var existingHedgingItem = scope.Model.HedgingItems.filter(function (obj) {
                            if (obj.ItemID == item.ItemID)
                                return true;
                        });

                        if (existingHedgedItem.length === 0 && item.HedgeRelationshipItemType === 'HedgedItem') {
                            scope.Model.HedgedItems.push(item);
                            //$scope.AddTradeLink(); //DE-4231 temporary remove auto-save functionality
                        }
                        else if (existingHedgingItem.length === 0 && item.HedgeRelationshipItemType === 'HedgingItem') {
                            scope.Model.HedgingItems.push(item);
                            //$scope.AddTradeLink(); //DE-4231 temporary remove auto-save functionality
                        }

                        setNotional(scope);

                        var obj = $("#" + item.HedgeRelationshipItemType + "Div").ejGrid("instance");
                        obj.dataSource(scope.Model[item.HedgeRelationshipItemType + 's']);

                        if (ngDialog !== undefined)
                            ngDialog.close();
                    });
            }
        };

        setNotional = function (scope) {
            scope.Model.Notional = 0;

            if (scope.Model.HedgingItems && scope.Model.HedgingItems.length > 0) {
                scope.Model.Notional = scope.Model.HedgingItems[scope.Model.HedgingItems.length - 1].Notional;
            }

            if (!scope.$$phase) {
                scope.$digest();
            }
        };

        $scope.validateGlAccount = function (newValue, field) {
            if ($scope.Model.HedgeType === "CashFlow" && $scope.Model.HedgeState === "Dedesignated") {
                $haService
                    .setUrl("HedgeRelationship/GLMapping")
                    .post($scope)
                    .then(function (response) {
                        if (field === "GLAccountID") {
                            if (newValue !== response.data.GlAccountId.toString()) {
                                if (!confirm("The default GL Account is used in HA JE Report. Proceed with the selected value anyway?")) {
                                    $scope.HedgeRelationshipOptionTimeValueAmort.GLAccountID = response.data.GlAccountId.toString();
                                }
                            }
                        }
                        else if (field === "ContraAccountID") {
                            if (newValue !== response.data.GlContraAcctId.toString()) {
                                if (!confirm("The default Contra GL Account is used in HA JE Report. Proceed with the selected value anyway?")) {
                                    $scope.HedgeRelationshipOptionTimeValueAmort.ContraAccountID = response.data.GlContraAcctId.toString();
                                }
                            }
                        }
                    });
            }
        };

        $scope.cancelsubmitAmortization = function () {
            ngDialog.close();
        };

        $scope.submitAmortization = function () {

            $scope.Model.HedgeRelationshipOptionTimeValueAmorts = [];

            var model = 'HedgeRelationshipOptionTimeValueAmort';
            var url = $haService
                .setUrl(model);

            var finacialCenters = $('#HedgeRelationshipOptionTimeValueAmort_FinancialCenters_input').val().split(',');

            $scope.HedgeRelationshipOptionTimeValueAmort.HedgeRelationship = $scope.Model;
            $scope.HedgeRelationshipOptionTimeValueAmort.FinancialCenters = finacialCenters;
            $scope.HedgeRelationshipOptionTimeValueAmort.OptionTimeValueAmortType = "Amortization";

            var msg = $scope.HedgeRelationshipOptionTimeValueAmort.ID > 0 ? "Success! Ammortization Updated." : "Success! Ammortization Created.";

            url.post($scope, model)
                .then(function (response) {
                    $scope.init(response.data.HedgeRelationshipID, function () {
                        $scope.relationshipUserMessage = msg;

                        if (!$scope.$$phase) {
                            $scope.$apply();
                        }

                        usermessage();

                        $timeout(function () {
                            $scope.cancelsubmitAmortization();
                            jQuery("#tabs-hedgeRelationship").tabs("option", "active", 3);
                        }, 2);
                    });
                });
        };

        $scope.checkUserRole = function (role) {
            var exists = false;

            if (role !== undefined && role !== null) {
                for (var i = 0; i < app.userRoles.length; i++) {
                    if (app.userRoles[i].toString() === role.toString()) {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        };

        $scope.disableSave = function () {
            return $scope.InProgress || (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) && $scope.Model.HedgeState !== 'Draft');
        };

        $scope.disablePrevInceptionPackage = function () {
            return $scope.InProgress
                || $scope.Model.LatestHedgeRegressionBatch === null
                || $scope.Model.LatestHedgeRegressionBatch === undefined
                || (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) && $scope.Model.HedgeState !== 'Draft');
        };

        $scope.disableRunRegression = function () {
            return $scope.InProgress
                || ($scope.Model.Benchmark === 'None' && $scope.Model.HedgeRiskType !== 'ForeignExchange')
                || (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) && $scope.Model.HedgeState !== 'Draft');
        };

        $scope.disableBackload = function () {
            return $scope.InProgress
                || !($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5'))
                || $scope.Model.HedgeState == 'Draft';
        };

        $scope.hideSelectNewOrRemoveTrade = function () {
            if ($scope.Model === undefined) {
                setTimeout(function () {
                    $scope.hideSelectNewOrRemoveTrade();
                }, 1000);
                return false;
            }
            else {
                return $scope.DesignatedIsDPIUser || (!($scope.checkUserRole('24') || $scope.checkUserRole('17') || $scope.checkUserRole('5')) && $scope.Model.HedgeState !== 'Draft');
            }
        };

        $scope.toggleCashPaymentType = function (cashPaymentType) {
            $scope.Model.DeDesignation.CashPaymentType = cashPaymentType;
            $scope.Model.FullCashPayment = cashPaymentType == 0;
            $scope.Model.PartialCashPayment = cashPaymentType == 1;
            $scope.Model.NoCashPayment = cashPaymentType == 2;

            /*DE-3277
            var enabled = (cashPaymentType == 0);*/
            var enabled = true;
            $('#deDesignateAmount').prop('disabled', !enabled);
            $('#deDesignateStartDate').ejDatePicker({ enabled: enabled });
            $('#deDesignateEndDate').ejDatePicker({ enabled: enabled });
        };

        $scope.toggleHedgedExposureExist = function (exist) {
            /*DE-3277
             * if ($scope.Model.CashPaymentType != 0) { return; }*/
            $scope.Model.DeDesignation.HedgedExposureExist = exist;
            $scope.Model.HedgedExposureNotExist = !exist;
            // enabled/disabled start/end input date
            if (exist === 0) {
                $('#deDesignateStartDate').ejDatePicker({ enabled: false });
                $('#deDesignateEndDate').ejDatePicker({ enabled: false });
            } else {
                $('#deDesignateStartDate').ejDatePicker({ enabled: true });
                $('#deDesignateEndDate').ejDatePicker({ enabled: true });
            }
        };

        $scope.toggleReason = function (reason) {
            $scope.Model.DedesignationReason = reason;
            $scope.Model.Termination = reason == 0;
            $scope.Model.Ineffectiveness = reason == 1;
            $haService
                .setUrl('HedgeRelationship/Dedesignate/' + id + "/" + reason)
                .get()
                .then(function (response) {
                    if (response.data === null) {
                        $scope.DedesignateDisabled = true;
                        if (reason === 0) {
                            $scope.DedesignateUserMessage = 'Status of Hedge Item is not Terminated.';
                            //alert('Status of Hedge Item is not Terminated.');
                        }
                    }
                    else if (response.data.ErrorMessage) {
                        $scope.DedesignateDisabled = true;
                        $scope.DedesignateUserMessage = response.data.ErrorMessage;
                        $scope.Model.DeDesignation.DedesignationDate = moment(response.data.DedesignationDate).format("M/D/YYYY");
                    }
                    else {
                        $scope.DedesignateDisabled = false;
                        $scope.DedesignateUserMessage = '';
                        $scope.Model.TimeValuesStartDate = moment(response.data.DedesignationDate).format('M/D/YYYY');
                        $scope.Model.TimeValuesEndDate = moment(response.data.TimeValuesEndDate).format('M/D/YYYY');
                        $scope.Model.FullCashPayment = true;
                        $scope.Model.PartialCashPayment = false;
                        $scope.Model.NoCashPayment = false;
                        $scope.Model.HedgedExposureNotExist = false;
                        $scope.Model.DeDesignation.DedesignationDate = moment(response.data.DedesignationDate).format("M/D/YYYY");
                        $scope.Model.DeDesignation.Payment = response.data.Payment;
                        $scope.Model.DeDesignation.ShowBasisAdjustmentBalance = response.data.ShowBasisAdjustmentBalance;
                        $scope.Model.DeDesignation.BasisAdjustment = response.data.BasisAdjustment;
                        $scope.Model.DeDesignation.BasisAdjustmentBalance = response.data.BasisAdjustmentBalance;
                        $scope.Model.DeDesignation.CashPaymentType = 0;
                        $scope.Model.DeDesignation.HedgedExposureExist = true;
                    }
                });
        };

        $scope.setTextBoxFromFields = function () {
            var html;

            if ($scope.Model.IsCaarHedgeTemplate && $scope.Model.ID > 0 ||
                ($scope.Model.IsNewHedgeDocumentTemplate && $scope.Model.IsCaarHedgeTemplate) || !$scope.Model.IsCaarHedgeTemplate) {
                html = $scope.Model.Objective !== undefined && $scope.Model.Objective !== null ? $scope.Model.Objective : '';

                setRichTextBoxControl('init', 'Objective', html);
                setRichTextBoxControl('detail', 'Objective', html);
            }

            html = $scope.Model.HedgedItemTypeDesc !== undefined && $scope.Model.HedgedItemTypeDesc !== null ? $scope.Model.HedgedItemTypeDesc : '';

            setRichTextBoxControl('init', 'HedgedItemTypeDesc', html);
            setRichTextBoxControl('detail', 'HedgedItemTypeDesc', html);
            return;
        };

        setRichTextBoxControl = function (div, field, value) {

            var obj = $('#' + div + field);

            obj.ejRTE({
                value: value,
                width: "100%",
                maxLength: 20000,
                //enableXHTML: true,
                tools: {
                    links: [],
                    importExport: ["import"]
                },
                importSettings: {
                    url: config.HAAPIUrl + "RTE/Import"
                },
                pasteCleanupSettings: {
                    listConversion: true,
                    cleanCSS: true,
                    removeStyles: true,
                    cleanElements: true
                },
                showHtmlSource: true,
                showFooter: true
            });

            var rteObj = $('#' + div + field).data("ejRTE");

            if (rteObj) {
                $("#" + rteObj.element[0].id + "_Iframe").contents().on("keyup", function (e) {
                    $scope.Model[field] = rteObj.model.value;
                    if (!$scope.$$phase) {
                        $scope.$apply();
                    }
                });
            }
        };

        $scope.setDetailFormatted = function (div, field) {

            var editor = $("#" + div + field).data("ejRTE");

            if (editor) {
                var encoded = $('<div />').text(editor.model.value).html();
                $('#' + div + field).val(encoded);
                $scope.Model[field] = editor.model.value;
            }

            if (!$scope.$$phase) {
                $scope.$apply();
            }
        };

        $scope.setFieldsFromTextBox = function (div) {
            if (!$scope.Model.HedgeDocumentTemplateName && !$scope.Model.IsNewHedgeDocumentTemplate || !$scope.Model.IsCaarHedgeTemplate) {
                $scope.setDetailFormatted(div, 'Objective');
                $scope.setDetailFormatted(div, 'HedgedItemTypeDesc');
            }
        };

        checkOptionPremium = function () {
            if ($scope.Model.IsAnOptionHedge) {
                if ($scope.Model.OptionPremium === undefined
                    || $scope.Model.OptionPremium === null
                    || parseFloat($scope.Model.OptionPremium) < 0) {
                    $scope.ha_errors.push("Option Premium must be greater than zero");
                }
            }
        };

        $scope.openOptionTimeValueAmortDialog = function () {
            $haService
                .setUrl("HedgeRelationship/GetOptionAmortizationDefaults")
                .post($scope)
                .then(function (response) {
                    $scope.OptionTimeValueAmortDefaults = response.data;
                    $scope.HedgeRelationshipOptionTimeValueAmort = {
                        ID: 0,
                        GLAccountID: $scope.OptionTimeValueAmortDefaults.GlAccountId,
                        ContraAccountID: $scope.OptionTimeValueAmortDefaults.GlContraAcctId,
                        AmortizationMethod: $scope.Model.AmortizationMethod,
                        FinancialCenters: ["USGS"],
                        PaymentFrequency: "Monthly",
                        DayCountConv: "ACT_360",
                        PayBusDayConv: "ModFollowing",
                        Straightline: $scope.Model.AmortizationMethod === "Straightline",
                        OptionTimeValueAmortType: "OptionTimeValue",
                        IVGLAccountID: $scope.OptionTimeValueAmortDefaults.GlAccountId2,
                        IVContraAccountID: $scope.OptionTimeValueAmortDefaults.GlContraAcctId2,
                        IVAmortizationMethod: $scope.OptionTimeValueAmortDefaults.IVAmortizationMethod,
                        IntrinsicValue: $scope.OptionTimeValueAmortDefaults.IntrinsicValue,
                        TotalAmount: $scope.OptionTimeValueAmortDefaults.TimeValue,
                        HedgeRelationship: $scope.Model,
                        AmortizeOptionPremimum: true
                    };

                    if ($scope.Model.HedgingItems.length > 0) {
                        $scope.HedgeRelationshipOptionTimeValueAmort.StartDate = $scope.Model.DesignationDate; // HAGL-149: Match to the designation date as per Jay's comment.
                        $scope.HedgeRelationshipOptionTimeValueAmort.EndDate = $scope.Model.HedgingItems[0].MaturityDate;
                    }

                    ngDialog.open({
                        template: "haOptionTimeValueAmort",
                        scope: $scope,
                        className: "ngdialog-theme-default ngdialog-theme-custom custom-height-800 amortizationAddEditDialog",
                        title: "Option Amortization",
                        showTitleCloseshowClose: true,
                        width: "800px",
                        height: "500px",
                        closeByEscape: false,
                        closeByDocument: false,
                        preCloseCallback: function () {
                            var totalAmountElement = document.getElementById('totalAmount');

                            if (totalAmountElement) {
                                totalAmountElement.removeEventListener('input', handleTotalAmountInput);
                            }

                            var intrinsicValueElement = document.getElementById('intrinsicValue');

                            if (intrinsicValueElement) {
                                intrinsicValueElement.removeEventListener('input', handleTotalAmountInput);
                            }
                            return true;
                        }
                    });

                    $timeout(function () {


                        ['totalAmount', 'intrinsicValue'].forEach(function (selector) {
                            waitForElement(selector, handleElementInput);
                        });
                    }, 0);

                });
        };


        $scope.cancelGenerateOptionTimeValueAmort = function () {
            ngDialog.close();
        };

        $scope.generateOptionTimeValueAmort = function () {
            $scope.Model.AmortizationMethod = $scope.HedgeRelationshipOptionTimeValueAmort.AmortizationMethod;
            $scope.Model.OptionPremium = $scope.HedgeRelationshipOptionTimeValueAmort.TotalAmount;

            if ($scope.HedgeRelationshipOptionTimeValueAmort.ID === 0) {
                $scope.HedgeRelationshipOptionTimeValueAmort.OptionTimeValueAmortType = 'OptionTimeValue';
            }
            else {
                $scope.Model.HedgeRelationshipOptionTimeValues = [];
            }

            var model = 'HedgeRelationshipOptionTimeValueAmort';
            var url = $haService
                .setUrl(model);

            var msg = $scope.HedgeRelationshipOptionTimeValueAmort.ID > 0 ? "Success! Schedule Updated." : "Success! Schedule Created.";

            url.post($scope, model)
                .then(function (response) {
                    $scope.init(response.data.HedgeRelationshipID, function () {
                        $scope.relationshipUserMessage = msg;

                        if (!$scope.$$phase) {
                            $scope.$apply();
                        }

                        usermessage();

                        $timeout(function () {
                            $scope.cancelGenerateOptionTimeValueAmort();
                            jQuery("#tabs-hedgeRelationship").tabs("option", "active", 5);
                        }, 2);
                    });
                });
        };

        $scope.AddTradeLink = function (final, callback) {

            var dateNow = new Date();
            var ID = $scope.Model.ID;

            $scope.ha_errors = [];
            $scope.relationshipUserMessage = "";
            $scope.HedgeRelationshipActivities = [];

            var item = {
                ID: ID,
                HedgeRelationshipID: ID,
                ActivityType: '12',
                Enabled: '1',
                CreatedOn: app.retreiveDatetoCorrectFormat(new Date(dateNow)),
                CreatedByID: '1',
                ModifiedOn: app.retreiveDatetoCorrectFormat(new Date(dateNow)),
                ModifiedByID: '1'
            };

            $scope.Model.HedgeRelationshipActivities.push(item);

            if ($scope.ha_errors.length === 0) {

                $haService
                    .setUrl('HedgeRelationship/AddTradeLink')
                    .post($scope)
                    .then(function (response) {

                        response.data;
                        $scope.relationshipUserMessage = "Success! Link trade was successfully added.";
                        $scope.Model.HedgeRelationshipActivities = null;

                        if (final) {
                            $scope.cancel();
                        }
                        else {
                            setTimeout(function () {
                                $scope.relationshipUserMessage = '';
                                $scope.$apply();
                            }, 3000);
                        }

                        $timeout(function () {
                            usermessage();

                            if (callback !== undefined) {
                                callback();
                            }
                        }, 0);
                    });
            }
        };

        $scope.$watch('Model.PeriodSize', function (new_, old_) {
            if (new_ !== undefined && old_ !== undefined) {
                if (!$scope.IsPeriodSizeMonth()) {
                    $scope.Model.EOM = false;
                }
            }
        });

        $scope.IsPeriodSizeMonth = function () {
            return $scope.Model.PeriodSize === "Month";
        }

        $scope.downloadSpecAndChecks = function () {
            $haService
                .setUrl('HedgeRelationship/DownloadSpecsAndChecks')
                .download($scope, undefined, 'HRSpecAndChecks', function (response) {

                });
        };

        $scope.previewHedgeDocumentObjective = function () {
            ngDialog.open({
                template: "hedgeDocumentObjectiveTemplate",
                scope: $scope,
                className: "ngdialog-theme-default ngdialog-theme-custom custom-height-800 amortizationAddEditDialog",
                title: "Hedge Document",
                showTitleCloseshowClose: true,
                width: "75%",
                height: "90%",
                closeByEscape: false,
                closeByDocument: false
            });
        };

        $scope.redirectToHedgeDocumentService = function (url) {
       
            $haService
                .setUrl('HedgeRelationship/SaveHrKeyword?hrId=' + $scope.Model.ID + '')
                .post($scope)
                .then(function () {
                    $haService
                        .setUrl('HedgeRelationship/SaveHrCacheData?hrId=' + $scope.Model.ID + '')
                        .post($scope)
                        .then(function (response) {
                            this.location.href = url + 'HedgeRelationshipId=' + $scope.Model.ID + '&ClientId=' + $scope.Model.ClientID + '';
                        });
            
                });
        };
    }]);