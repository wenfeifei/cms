﻿using System;
using System.Collections.Generic;
using SS.CMS.Enums;
using SS.CMS.Repositories;
using SS.CMS.Utils;

namespace SS.CMS.Core.Common
{
    public class InputTypeUtils
    {
        public static string GetText(InputType type)
        {
            if (type == InputType.CheckBox)
            {
                return "复选框";
            }
            if (type == InputType.Radio)
            {
                return "单选框";
            }
            if (type == InputType.SelectOne)
            {
                return "下拉列表(单选)";
            }
            if (type == InputType.SelectMultiple)
            {
                return "下拉列表(多选)";
            }
            if (type == InputType.SelectCascading)
            {
                return "下拉列表(级联)";
            }
            if (type == InputType.Date)
            {
                return "日期选择框";
            }
            if (type == InputType.DateTime)
            {
                return "日期时间选择框";
            }
            if (type == InputType.Image)
            {
                return "图片";
            }
            if (type == InputType.Video)
            {
                return "视频";
            }
            if (type == InputType.File)
            {
                return "附件";
            }
            if (type == InputType.Text)
            {
                return "文本框(单行)";
            }
            if (type == InputType.TextArea)
            {
                return "文本框(多行)";
            }
            if (type == InputType.TextEditor)
            {
                return "内容编辑器";
            }
            if (type == InputType.Customize)
            {
                return "自定义";
            }
            if (type == InputType.Hidden)
            {
                return "隐藏";
            }

            throw new Exception();
        }

        public static bool Equals(InputType type, string typeStr)
        {
            if (string.IsNullOrEmpty(typeStr)) return false;
            if (string.Equals(type.Value.ToLower(), typeStr.ToLower()))
            {
                return true;
            }
            return false;
        }

        public static bool Equals(string typeStr, InputType type)
        {
            return Equals(type, typeStr);
        }

        public static bool EqualsAny(InputType type, params InputType[] types)
        {
            foreach (var theType in types)
            {
                if (type == theType)
                {
                    return true;
                }
            }
            return false;
        }



        public static bool IsWithStyleItems(InputType type)
        {
            if (type == InputType.CheckBox || type == InputType.Radio || type == InputType.SelectMultiple || type == InputType.SelectOne || type == InputType.SelectCascading)
            {
                return true;
            }
            return false;
        }

        public static bool IsPureString(InputType type)
        {
            if (type == InputType.Date || type == InputType.DateTime || type == InputType.CheckBox || type == InputType.Radio || type == InputType.SelectMultiple || type == InputType.SelectOne || type == InputType.Image || type == InputType.Video || type == InputType.File || type == InputType.SelectCascading)
            {
                return false;
            }
            return true;
        }

        public static List<KeyValuePair<InputType, string>> GetInputTypes(IUserRepository userRepository, string tableName)
        {
            if (tableName == userRepository.TableName)
            {
                return new List<KeyValuePair<InputType, string>>
                {
                    new KeyValuePair<InputType, string>(InputType.Text, GetText(InputType.Text)),
                    new KeyValuePair<InputType, string>(InputType.TextArea, GetText(InputType.TextArea)),
                    new KeyValuePair<InputType, string>(InputType.CheckBox, GetText(InputType.CheckBox)),
                    new KeyValuePair<InputType, string>(InputType.Radio, GetText(InputType.Radio)),
                    new KeyValuePair<InputType, string>(InputType.SelectOne, GetText(InputType.SelectOne)),
                    new KeyValuePair<InputType, string>(InputType.SelectMultiple, GetText(InputType.SelectMultiple)),
                    new KeyValuePair<InputType, string>(InputType.Date, GetText(InputType.Date)),
                    new KeyValuePair<InputType, string>(InputType.DateTime, GetText(InputType.DateTime)),
                    new KeyValuePair<InputType, string>(InputType.Image, GetText(InputType.Image)),
                    new KeyValuePair<InputType, string>(InputType.Video, GetText(InputType.Video)),
                    new KeyValuePair<InputType, string>(InputType.File, GetText(InputType.File)),
                    new KeyValuePair<InputType, string>(InputType.Hidden, GetText(InputType.Hidden))
                };
            }

            return new List<KeyValuePair<InputType, string>>
            {
                new KeyValuePair<InputType, string>(InputType.Text, GetText(InputType.Text)),
                new KeyValuePair<InputType, string>(InputType.TextArea, GetText(InputType.TextArea)),
                new KeyValuePair<InputType, string>(InputType.TextEditor, GetText(InputType.TextEditor)),
                new KeyValuePair<InputType, string>(InputType.CheckBox, GetText(InputType.CheckBox)),
                new KeyValuePair<InputType, string>(InputType.Radio, GetText(InputType.Radio)),
                new KeyValuePair<InputType, string>(InputType.SelectOne, GetText(InputType.SelectOne)),
                new KeyValuePair<InputType, string>(InputType.SelectMultiple, GetText(InputType.SelectMultiple)),
                new KeyValuePair<InputType, string>(InputType.SelectCascading, GetText(InputType.SelectCascading)),
                new KeyValuePair<InputType, string>(InputType.Date, GetText(InputType.Date)),
                new KeyValuePair<InputType, string>(InputType.DateTime, GetText(InputType.DateTime)),
                new KeyValuePair<InputType, string>(InputType.Image, GetText(InputType.Image)),
                new KeyValuePair<InputType, string>(InputType.Video, GetText(InputType.Video)),
                new KeyValuePair<InputType, string>(InputType.File, GetText(InputType.File)),
                new KeyValuePair<InputType, string>(InputType.Customize, GetText(InputType.Customize)),
                new KeyValuePair<InputType, string>(InputType.Hidden, GetText(InputType.Hidden))
            };
        }

        public static string ParseString(InputType inputType, string content, string replace, string to, int startIndex, int length, int wordNum, string ellipsis, bool isClearTags, bool isReturnToBr, bool isLower, bool isUpper, string formatString)
        {
            return IsPureString(inputType) ? ParseString(content, replace, to, startIndex, length, wordNum, ellipsis, isClearTags, isReturnToBr, isLower, isUpper, formatString) : content;
        }

        private static string ParseString(string content, string replace, string to, int startIndex, int length, int wordNum, string ellipsis, bool isClearTags, bool isReturnToBr, bool isLower, bool isUpper, string formatString)
        {
            var parsedContent = content;

            if (!string.IsNullOrEmpty(replace))
            {
                parsedContent = StringUtils.ParseReplace(parsedContent, replace, to);
            }

            if (isClearTags)
            {
                parsedContent = StringUtils.StripTags(parsedContent);
            }

            if (!string.IsNullOrEmpty(parsedContent))
            {
                if (startIndex > 0 || length > 0)
                {
                    try
                    {
                        parsedContent = length > 0 ? parsedContent.Substring(startIndex, length) : parsedContent.Substring(startIndex);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                if (wordNum > 0)
                {
                    parsedContent = StringUtils.MaxLengthText(parsedContent, wordNum, ellipsis);
                }

                if (isReturnToBr)
                {
                    parsedContent = StringUtils.ReplaceNewlineToBr(parsedContent);
                }

                if (!string.IsNullOrEmpty(formatString))
                {
                    parsedContent = string.Format(formatString, parsedContent);
                }

                if (isLower)
                {
                    parsedContent = parsedContent.ToLower();
                }
                if (isUpper)
                {
                    parsedContent = parsedContent.ToUpper();
                }
            }

            return parsedContent;
        }
    }
}
