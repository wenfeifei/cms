﻿using System;
using System.IO;
using System.Reflection;
using SS.CMS.Core.Common.Office;
using SS.CMS.Utils;
using SS.CMS.Utils.Tests;
using Xunit;

namespace SS.CMS.Core.Tests.Common.Office
{
    public class WordManagerTests : IClassFixture<UnitTestsFixture>
    {
        private UnitTestsFixture _fixture { get; }

        public WordManagerTests(UnitTestsFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void WordParseTest()
        {
            var projDirectoryPath = _fixture.SettingsManager.ContentRootPath;

            var htmlDirectoryPath = PathUtils.Combine(projDirectoryPath, "output");
            var imageDirectoryPath = PathUtils.Combine(htmlDirectoryPath, "images");
            const string imageDirectoryUrl = "images";
            DirectoryUtils.DeleteDirectoryIfExists(htmlDirectoryPath);
            DirectoryUtils.CreateDirectoryIfNotExists(htmlDirectoryPath);

            var wordsDirectory = PathUtils.Combine(projDirectoryPath, "assets/words");

            foreach (var docxFilePath in Directory.GetFiles(wordsDirectory, "*.docx"))
            {
                var settings = new WordManager.ConverterSettings
                {
                    IsSaveHtml = true,
                    HtmlDirectoryPath = htmlDirectoryPath,
                    ImageDirectoryPath = imageDirectoryPath,
                    ImageDirectoryUrl = imageDirectoryUrl
                };

                WordManager.ConvertToHtml(docxFilePath, settings);
            }
            foreach (var file in Directory.GetFiles(htmlDirectoryPath, "*.html"))
            {
                WordManager.ConvertToDocx(file, htmlDirectoryPath);
            }

            Assert.True(true);
        }
    }
}
