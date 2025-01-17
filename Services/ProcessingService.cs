﻿using VoiceGradeApi.Services.FileService;
using VoiceGradeApi.Services.AudioConvertService;
using VoiceGradeApi.Util;

namespace VoiceGradeApi.Services;

public class ProcessingService
{
    private IFileService _fileService;
    private TextParser _parser;
    private Correlation _correlation;

    public ProcessingService()
    {
        _parser = new TextParser();
        _correlation = new Correlation();
    }

    public string GetResultedFile(List<string> files, TranscriberService transcriberService)
    {
        string audioFile = "", pupilsFile = "";
        foreach (var file in files)
        {
            var info = new FileInfo(file);
            switch (info.Extension)
            {
                case ".json":
                    _fileService = new JsonService();
                    pupilsFile = file;
                    break;
                case ".xml":
                    _fileService = new XmlService();
                    pupilsFile = file;
                    break;
                case ".wav": case ".mp3": case ".m4a":
                    audioFile = file;
                    break;
            }
        }
        
        if (pupilsFile == "") throw new BadHttpRequestException("Need to load file with pupils");
        if (audioFile == "") throw new BadHttpRequestException("Need to load audiofile");
        
        AudioConverter converter = new MpConverter(audioFile);
        audioFile = converter.ConvertAudio();
        var pupils = _fileService.ReadFile(pupilsFile);
        var allData = TranscriberService.TranscribeAudio(audioFile);
        var transcribedNames = _parser.ParseData(allData);
        _correlation.CorrelateScores(pupils, transcribedNames);
        var createdFilePath = _fileService.CreateFile(pupils);
        return createdFilePath;
    }
}