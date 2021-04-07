#ifndef LOGIN_HANDLER_H
#define LOGIN_HANDLER_H

#include "generated/Login.h"

class LoginHandler : public LoginIf {
    unsigned connectionId;
    bool loggedIn;

    int32_t getKey(const std::string& username) const;

public:
    explicit LoginHandler(unsigned connectionId): connectionId(connectionId), loggedIn(false) {}

    void loginGuard() const;
    void logIn(const std::string& username, const int32_t key) override;
    void logOut() override;
};

#endif