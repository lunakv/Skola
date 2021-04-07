#include "ReportsHandler.h"

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
            addField("fieldA", std::to_string(a.fieldA), report);
            addField("fieldB", stringify(a.fieldB), report);
            addField("fieldC", std::to_string(a.fieldC), report);
        } else if (item.__isset.itemB) {
            const ItemB& b(item.itemB);
            addField("fieldA", b.fieldA, report);
            addField("fieldB", stringify(b.fieldB), report);
            if (b.__isset.fieldC)
                addField("fieldC", stringify(b.fieldC), report);
        } else if (item.__isset.itemC) {
            addField("fieldA", std::to_string(item.itemC.fieldA), report);
        }
    }
    return report;
}

bool ReportsHandler::saveReport(const Report& report) {
    loginHandler->loginGuard();
    Report expected = generateReport(searchHandler->getQueryResults());
    return expected == report;
}