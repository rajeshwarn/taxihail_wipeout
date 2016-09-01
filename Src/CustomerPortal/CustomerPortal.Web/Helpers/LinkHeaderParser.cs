﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomerPortal.Web.Helpers
{
    public class WebLinkParser
    {
        public static string ParseAndGetNextLink(string linkHeader)
        {
            var parser = new WebLinkParser();
            var list = parser.Parse(linkHeader);
            var nextLink = list.FirstOrDefault(x => x.Relation == "next");

            return nextLink != null ? nextLink.Url : null;
        }
        
        public IList<LinkResult> Parse(string linkHeader)
        {
            InputString = linkHeader;
            InputPos = 0;

            var links = new List<LinkResult>();

            while (true)
            {
                try
                {
                    GetNextToken();

                    if (NextToken.Type == TokenType.Url)
                        links.Add(ParseLink());
                    else if (NextToken.Type == TokenType.EOF)
                        break;
                    else
                        Error(string.Format("Unexpected token '{0}' (expected URL)", NextToken.Type));

                    if (NextToken.Type == TokenType.Comma)
                        continue;
                    else if (NextToken.Type == TokenType.EOF)
                        break;
                    else
                        Error(string.Format("Unexpected token '{0}' (expected comma)", NextToken.Type));
                }
                catch (FormatException)
                {
                    while (NextToken.Type != TokenType.Comma && NextToken.Type != TokenType.EOF)
                    {
                        try
                        {
                            GetNextToken();
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }
            }

            return links;
        }


        #region Parser

        protected LinkResult ParseLink()
        {
            string url = NextToken.Value;
            string rel = null;
            string title = null;
            string title_s = null;
            string type = null;

            GetNextToken();

            while (NextToken.Type == TokenType.Semicolon)
            {
                try
                {
                    GetNextToken();
                    bool isExtended;
                    KeyValuePair<string, string> p = ParseParameter(out isExtended);

                    if (p.Key == "rel" && rel == null)
                        rel = p.Value;
                    else if (p.Key == "title" && title == null && !isExtended)
                        title = p.Value;
                    else if (p.Key == "title" && title_s == null && isExtended)
                        title_s = p.Value;
                    else if (p.Key == "type" && type == null)
                        type = p.Value;
                }
                catch (FormatException)
                {
                    while (NextToken.Type != TokenType.Semicolon && NextToken.Type != TokenType.Comma && NextToken.Type != TokenType.EOF)
                    {
                        try
                        {
                            GetNextToken();
                        }
                        catch (FormatException)
                        {
                        }
                    }
                }
            }

            var link = new LinkResult { Url = url, Relation = rel };
            return link;
        }

        private KeyValuePair<string, string> ParseParameter(out bool isExtended)
        {
            if (NextToken.Type != TokenType.Identifier && NextToken.Type != TokenType.ExtendedIdentifier)
                Error(string.Format("Unexpected token '{0}' (expected an identifier)", NextToken.Type));
            string id = NextToken.Value;
            isExtended = (NextToken.Type == TokenType.ExtendedIdentifier);
            GetNextToken();

            if (NextToken.Type != TokenType.Assignment)
                Error(string.Format("Unexpected token '{0}' (expected an assignment)", NextToken.Type));

            if (id == "rel")
            {
                GetNextStringOrRelType();
            }
            else
            {
                GetNextToken();
            }

            if (NextToken.Type != TokenType.String)
                Error(string.Format("Unexpected token '{0}' (expected an string)", NextToken.Type));
            string value = NextToken.Value;
            GetNextToken();

            return new KeyValuePair<string, string>(id, value);
        }


        #endregion


        #region Token scanner

        protected Token NextToken { get; set; }

        protected enum TokenType { Url, Semicolon, Comma, Assignment, Identifier, ExtendedIdentifier, String, EOF }

        protected class Token
        {
            public TokenType Type { get; set; }
            public string Value { get; set; }
        }

        protected string InputString { get; set; }
        protected int InputPos { get; set; }


        protected void GetNextToken()
        {
            NextToken = ReadToken();
        }


        protected void GetNextStringOrRelType()
        {
            NextToken = ReadNextStringOrRelType();
        }


        protected Token ReadToken()
        {
            while (true)
            {
                char? c = ReadNextChar();

                if (c == null)
                    return new Token { Type = TokenType.EOF };

                if (c == ';')
                    return new Token { Type = TokenType.Semicolon };

                if (c == ',')
                    return new Token { Type = TokenType.Comma };

                if (c == '=')
                    return new Token { Type = TokenType.Assignment };

                if (c == '"')
                    return new Token { Type = TokenType.String, Value = ReadString() };

                if (c == '<')
                    return new Token { Type = TokenType.Url, Value = ReadUrl() };

                if (Char.IsWhiteSpace(c.Value))
                    continue;

                if (Char.IsLetter(c.Value))
                    return ReadIdentifier(c.Value);

                Error(string.Format("Unrecognized character '{0}'", c));
            }
        }


        protected Token ReadNextStringOrRelType()
        {
            while (true)
            {
                char? c = ReadNextChar();

                if (c == null)
                    return new Token { Type = TokenType.EOF };

                if (c == '"')
                    return new Token { Type = TokenType.String, Value = ReadString() };

                if (Char.IsLetter(c.Value))
                    return new Token { Type = TokenType.String, Value = ReadRelType(c.Value) };

                Error(string.Format("Unrecognized character '{0}' for string or rel-type", c));
            }
        }


        protected string ReadString()
        {
            string result = "";

            while (true)
            {
                char? c = ReadNextChar();
                if (c == null)
                    break;
                if (c == '"')
                    break;
                result += c;
            }

            return result;
        }


        protected string ReadUrl()
        {
            string result = "";

            while (true)
            {
                char? c = ReadNextChar();
                if (c == null)
                    break;
                if (c == '>')
                    break;
                result += c;
            }

            return result;
        }


        protected Token ReadIdentifier(char c)
        {
            string id = "" + c;

            while (Char.IsLetterOrDigit(InputString[InputPos]))
            {
                id += InputString[InputPos++];
            }

            if (InputString[InputPos] == '*')
            {
                InputPos++;
                return new Token { Type = TokenType.ExtendedIdentifier, Value = id };
            }
            else
            {
                return new Token { Type = TokenType.Identifier, Value = id };
            }
        }


        protected string ReadRelType(char c)
        {
            string id = "" + c;

            while (Char.IsLetterOrDigit(InputString[InputPos]) || InputString[InputPos] == '.' || InputString[InputPos] == '-')
            {
                id += InputString[InputPos++];
            }

            return id;
        }


        protected char? ReadNextChar()
        {
            if (InputPos == InputString.Length)
                return null;
            return InputString[InputPos++];
        }

        #endregion


        protected void Error(string msg)
        {
            throw new FormatException(string.Format("Invalid HTTP Web Link. {0} in '{1}' (around pos {2}).", msg, InputString, InputPos));
        }
    }

    public class LinkResult
    {
        public string Url { get; set; }
        public string Relation { get; set; }
    }
}