#include "pch.h"
#include "ShortcutGuideLogger.h"
#include <common\settings_helpers.h>
#include <filesystem>

std::shared_ptr<Logger> ShortcutGuideLogger::logger;

void ShortcutGuideLogger::Init(std::wstring moduleSaveLocation)
{
    std::filesystem::path logFilePath(moduleSaveLocation);
    logFilePath.append(LogSettings::shortcutGuideLogPath);
    logger = std::make_shared<Logger>(LogSettings::shortcutGuideLoggerName, logFilePath.wstring(), PTSettingsHelper::get_log_settings_file_location());
    logger->info("Shortcut Guide logger initialized");
}

std::shared_ptr<Logger> ShortcutGuideLogger::GetLogger()
{
    if (!logger)
    {
        throw "Shortcut Guide logger is not initialized";
    }

    return logger;
}
