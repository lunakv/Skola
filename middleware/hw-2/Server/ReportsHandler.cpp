#include "ReportsHandler.h"
#include <iostream>

/* string utility functions */
template<class T>
std::string toString(const T& val) {
    return std::to_string(val);
}
std::string toString(const std::string& str) {
    return str;
}
std::string toString(short val) {
    return std::to_string(int(val));
}
std::string toString(bool val) {
    return val ? "true" : "false";
}
template<class T>
std::string stringify(const T& val) {
    std::string s;
    for (const auto& i : val) {
        if (!s.empty())
            s += ",";
        s += toString(i);
    }
    return s;
}
/* string utility functions */

void ReportsHandler::addField(const std::string& key, const std::string& field, Report& report) const {
    if (!report.count(key)) {
        report.emplace(key, std::set<std::string>());
    }
    report[key].insert(field);
}

Report ReportsHandler::generateReport(const std::vector<Item>& results) const {
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