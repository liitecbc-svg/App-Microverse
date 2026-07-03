package com.microverse.translation;

public interface MlKitModelDownloadCallback {
    void onProgress(float progress, String languageCode);
    void onSuccess();
    void onError(String message);
    void onCancelled();
}
