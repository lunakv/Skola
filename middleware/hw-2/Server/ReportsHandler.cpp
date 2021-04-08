#include "ReportsHandler.h"

using namespace std;

/* string utility functions */
template<class T>
string toString(const T& val) {
    return to_string(val);
}
string toString(const string& str) {
    return str;
}
string toString(short val) {
    return to_string(int(val));
}
string toString(bool val) {
    return val ? "true" : "false";
}
template<class T>
string stringify(const T& val) {
    string s;
    for (const auto& i : val) {
        if (!s.empty())
            s += ",";
        s += toString(i);
    }
    return s;
}
/* string utility functions */

void ReportsHandler::addField(const string& key, const string& field, Report& report) const {
    if (!report.count(key)) {
        report.emplace(key, set<string>());
    }
    report[key].insert(field);
}

Report ReportsHandler::generateReport(const vector<Item>& results) const {
    Report report;
    for (const Item& item : results) {
        if (item.__isset.itemA) {
            const ItemA& a(item.itemA);
            addField("fieldA", toString(a.fieldA), report);
            addField("fieldB", stringify(a.fieldB), report);
            addField("fieldC", toString(a.fieldC), report);
        } else if (item.__isset.itemB) {
            const ItemB& b(item.itemB);
            addField("fieldA", b.fieldA, report);
            addField("fieldB", stringify(b.fieldB), report);
            if (b.__isset.fieldC)
                addField("fieldC", stringify(b.fieldC), report);
        } else if (item.__isset.itemC) {
            addField("fieldA", toString(item.itemC.fieldA), report);
        } else if (item.__isset.itemD) {
            // UPDATE: add report generation for ItemD
            const ItemD& d(item.itemD);
            addField("fieldA", toString(d.fieldA), report);
            if (d.__isset.fieldB)
                addField("fieldB", stringify(d.fieldB), report);
            addField("fieldC", d.fieldC, report);
        }
    }
    return report;
}

bool ReportsHandler::saveReport(const Report& report) {
    loginHandler->loginGuard();
    if (!searchHandler->startedSearch()) {
        ProtocolException ex;
        ex.__set_message("Cannot save report before initiating search.");
        throw ex;
    }
    Report expected = generateReport(searchHandler->getQueryResults());
    return expected == report;
}