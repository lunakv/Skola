#ifndef REPORTS_HANDLER_H
#define REPORTS_HANDLER_H

#include "generated/Reports.h"
#include "LoginHandler.h"
#include "SearchHandler.h"

class ReportsHandler : public ReportsIf {
    std::shared_ptr<LoginHandler> loginHandler;
    std::shared_ptr<SearchHandler> searchHandler;

    void addField(const std::string& key, const std::string& field, Report& report) const;
    Report generateReport(const std::vector<Item>& results) const;

public:
    ReportsHandler(std::shared_ptr<LoginHandler> loginHandler, std::shared_ptr<SearchHandler> searchHandler) :
        loginHandler(std::move(loginHandler)),
        searchHandler(std::move(searchHandler)) {}

    bool saveReport(const Report& report) override;
};

#endif