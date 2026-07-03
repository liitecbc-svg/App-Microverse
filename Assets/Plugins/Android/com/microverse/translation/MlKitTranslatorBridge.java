package com.microverse.translation;

import android.app.Activity;

import com.google.mlkit.common.model.DownloadConditions;
import com.google.mlkit.nl.translate.TranslateLanguage;
import com.google.mlkit.nl.translate.Translation;
import com.google.mlkit.nl.translate.Translator;
import com.google.mlkit.nl.translate.TranslatorOptions;

import java.util.HashMap;
import java.util.Map;

public class MlKitTranslatorBridge {
    private final Activity activity;
    private final Map<String, Translator> translators = new HashMap<>();
    private boolean modelPreparationCancelled = false;

    public MlKitTranslatorBridge(Activity activity) {
        this.activity = activity;
    }

    public void translateBatch(String[] texts, String[] sourceLanguages, String[] targetLanguages, MlKitTranslationCallback callback) {
        if (texts == null || sourceLanguages == null || targetLanguages == null) {
            callback.onError("Translation arrays must not be null.");
            return;
        }

        if (texts.length != sourceLanguages.length || texts.length != targetLanguages.length) {
            callback.onError("Translation arrays must have the same length.");
            return;
        }

        String[] results = new String[texts.length];
        translateAtIndex(0, texts, sourceLanguages, targetLanguages, results, callback);
    }

    public void prepareModels(String sourceLanguage, String[] targetLanguages, MlKitModelDownloadCallback callback) {
        if (targetLanguages == null) {
            callback.onError("Target languages must not be null.");
            return;
        }

        modelPreparationCancelled = false;
        runOnUiThread(() -> callback.onProgress(0f, ""));
        prepareModelAtIndex(0, normalizeLanguage(sourceLanguage), targetLanguages, callback);
    }

    public void cancelModelPreparation() {
        modelPreparationCancelled = true;
    }

    public void close() {
        for (Translator translator : translators.values()) {
            translator.close();
        }

        translators.clear();
    }

    private void prepareModelAtIndex(int index, String source, String[] targetLanguages, MlKitModelDownloadCallback callback) {
        if (modelPreparationCancelled) {
            runOnUiThread(callback::onCancelled);
            return;
        }

        if (index >= targetLanguages.length) {
            runOnUiThread(() -> callback.onProgress(1f, ""));
            runOnUiThread(callback::onSuccess);
            return;
        }

        String target = normalizeLanguage(targetLanguages[index]);
        if (source.equals(target)) {
            float progress = (index + 1f) / targetLanguages.length;
            runOnUiThread(() -> callback.onProgress(progress, target));
            prepareModelAtIndex(index + 1, source, targetLanguages, callback);
            return;
        }

        Translator translator = getTranslator(source, target);
        if (translator == null) {
            callback.onError("Unsupported ML Kit language pair: " + source + " -> " + target);
            return;
        }

        runOnUiThread(() -> callback.onProgress((float) index / targetLanguages.length, target));
        DownloadConditions conditions = new DownloadConditions.Builder().build();
        translator.downloadModelIfNeeded(conditions)
            .addOnSuccessListener(unused -> {
                float progress = (index + 1f) / targetLanguages.length;
                runOnUiThread(() -> callback.onProgress(progress, target));
                prepareModelAtIndex(index + 1, source, targetLanguages, callback);
            })
            .addOnFailureListener(exception -> callback.onError(exception.getMessage()));
    }

    private void translateAtIndex(
        int index,
        String[] texts,
        String[] sourceLanguages,
        String[] targetLanguages,
        String[] results,
        MlKitTranslationCallback callback
    ) {
        if (index >= texts.length) {
            runOnUiThread(() -> callback.onSuccess(results));
            return;
        }

        String text = texts[index] == null ? "" : texts[index];
        String source = normalizeLanguage(sourceLanguages[index]);
        String target = normalizeLanguage(targetLanguages[index]);

        if (text.length() == 0 || source.equals(target)) {
            results[index] = text;
            translateAtIndex(index + 1, texts, sourceLanguages, targetLanguages, results, callback);
            return;
        }

        Translator translator = getTranslator(source, target);
        if (translator == null) {
            callback.onError("Unsupported ML Kit language pair: " + source + " -> " + target);
            return;
        }

        DownloadConditions conditions = new DownloadConditions.Builder().build();
        translator.downloadModelIfNeeded(conditions)
            .continueWithTask(task -> {
                if (!task.isSuccessful()) {
                    throw task.getException() == null ? new Exception("Model download failed.") : task.getException();
                }

                return translator.translate(text);
            })
            .addOnSuccessListener(translatedText -> {
                results[index] = translatedText;
                translateAtIndex(index + 1, texts, sourceLanguages, targetLanguages, results, callback);
            })
            .addOnFailureListener(exception -> callback.onError(exception.getMessage()));
    }

    private Translator getTranslator(String source, String target) {
        String sourceCode = toMlKitLanguage(source);
        String targetCode = toMlKitLanguage(target);
        if (sourceCode == null || targetCode == null) {
            return null;
        }

        String key = sourceCode + ">" + targetCode;
        Translator existing = translators.get(key);
        if (existing != null) {
            return existing;
        }

        TranslatorOptions options = new TranslatorOptions.Builder()
            .setSourceLanguage(sourceCode)
            .setTargetLanguage(targetCode)
            .build();
        Translator translator = Translation.getClient(options);
        translators.put(key, translator);
        return translator;
    }

    private String normalizeLanguage(String value) {
        if (value == null) {
            return "es";
        }

        String lower = value.toLowerCase();
        int separator = lower.indexOf('-');
        if (separator >= 0) {
            return lower.substring(0, separator);
        }

        return lower;
    }

    private String toMlKitLanguage(String language) {
        switch (language) {
            case "en":
                return TranslateLanguage.ENGLISH;
            case "es":
                return TranslateLanguage.SPANISH;
            case "pt":
                return TranslateLanguage.PORTUGUESE;
            default:
                return TranslateLanguage.fromLanguageTag(language);
        }
    }

    private void runOnUiThread(Runnable runnable) {
        if (activity == null) {
            runnable.run();
            return;
        }

        activity.runOnUiThread(runnable);
    }
}
