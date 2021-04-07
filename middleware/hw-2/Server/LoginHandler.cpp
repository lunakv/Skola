#include "LoginHandler.h"

int32_t LoginHandler::getKey(const std::string& username) const {
    int32_t key{};
    for (char i : username) {
        key += int32_t(i);
        key %= 10000;
    }
    return key;
}

void LoginHandler::loginGuard() const {
    if (!loggedIn) {
        ProtocolException ex;
        ex.__set_message("Connection isn't logged in.");
        throw ex;
    }
}

void LoginHandler::logIn(const std::string& username, const int32_t key) {
    if (loggedIn) {
        ProtocolException ex;
        ex.__set_message("Connection already logged in.");
        throw ex;
    }
    int32_t expected = getKey(username);
    if (expected == key) {
        loggedIn = true;
    } else {
        InvalidKeyException ex = InvalidKeyException();
        ex.expectedKey = expected;
        ex.invalidKey = key;
        throw ex;
    }
}

void LoginHandler::logOut() {
    loginGuard();
    loggedIn = false;
}