package com.microverse.translation;

public interface MlKitTranslationCallback {
    void onSuccess(String[] translations);
    void onError(String message);
}
