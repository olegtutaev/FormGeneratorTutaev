﻿using System;
using Newtonsoft.Json;
using Newtonsoft;
using System.IO;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office.CustomUI;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSON_Tutaev
{
    public class Button : PageItem
    {
        public string Class { get; set; }
        public string Text { get; set; }

        public override string ToHtmlStr()
        {
            var tmpRes = string.Format("<p><button class = \"{0}\" >{1}</button></p>\n", Class, Text);
            return tmpRes;
        }
    }

    public class Checkbox : PageItem
    {
        public string Label { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
        public bool Disabled { get; set; }
        public override string ToHtmlStr()
        {
            var Result = new System.Text.StringBuilder("<p><b>");
            Result.Append(Label).Append("</b><br>\n");
            Result.AppendFormat("<input type = \"checkbox\" name = \"{0}\" value = \"{1}\" class = \"{2}\" ", Name, Value, Class);
            if (Required) Result.Append("Required ");
            if (Disabled) Result.Append("Disabled");
            if (Checked) Result.Append("Checked");
            Result.Append(">").Append(Value).Append("\n");
            Result.Append(" </p>\n");
            return Result.ToString();
        }
    }

    public class Filler : PageItem
    {
        public string Message { get; set; }
        public override string ToHtmlStr()
        {
            return "<p>" + Message + "</p>\n";
        }
    }

    public class Form
    {
        public string Name { get; set; }
        public string PostMessage { get; set; }
        public List<PageItem> Items { get; set; }

        public void ReadFromJson(string path)
        {
            var str = File.ReadAllText(path);
            Form tmp = JsonConvert.DeserializeObject<Form>(str, new ItemConverter());
            Name = tmp.Name;
            PostMessage = tmp.PostMessage;
            Items = tmp.Items;
        }

        public void WriteToHtml(string path)
        {
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                sw.Write("<!DOCTYPE html >\n" +
                    "<html lang=\"ru\" dir=\"ltr\">\n" +
                    "<head>\n" +
                    "<meta charset=\"utf - 8\">\n" +
                    "<title>Form generator</title>\n" +
                    "</head>\n" +
                    "<body>\n" +
                    "<form name=\"" + Name + "\">\n" +
                    "<h2>" + Name + "</h2>\n");
                foreach (var element in Items)
                {
                    sw.Write(element.ToHtmlStr());
                }
                sw.Write("</form>\n" +
                        "</body>\n" +
                        "</ html >");
            }
        }
    }

    public class ItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(PageItem).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);

            var type = (string)jo["Type"];

            PageItem item;

            switch (type)
            {
                case "filler":
                    item = new Filler();
                    break;
                case "text":
                    item = new Text();
                    break;
                case "textArea":
                    item = new TextArea();
                    break;
                case "checkbox":
                    item = new Checkbox();
                    break;
                case "button":
                    item = new Button();
                    break;
                case "select":
                    item = new Select();
                    break;
                case "radio":
                    item = new Radio();
                    break;
                default:
                    item = null;
                    break;
            }

            serializer.Populate(jo.CreateReader(), item);

            return item;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer,
            object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class PageItem
    {
        public string Type { get; set; }
        public abstract string ToHtmlStr();
    }

    public class Radio : PageItem
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Class { get; set; }
        public bool Disabled { get; set; }
        public string[,] Items { get; set; }
        public List<Item> LItems { get; private set; }
        public void FillList()
        {
            LItems = new List<Item>();
            for (var i = 0; i < Items.Length / 3; i++)
            {
                LItems.Add(new Item(Items[i, 0], Items[i, 1], Items[i, 2]));
            }
        }
        public override string ToHtmlStr()
        {
            var Result = new StringBuilder("<p><b>");
            Result.Append(Label).Append("</b><br>\n");
            if (LItems == null || LItems.Count == 0)
            {
                FillList();
            }
            foreach (var i in LItems)
            {
                Result.AppendFormat("<input type=\"radio\" name=\"{0}\" class = \"{1}\" value=\"{2}\" ", Name, Class, i.Value);
                if (Disabled)
                {
                    Result.Append("disabled ");
                }
                if (i.Checked)
                {
                    Result.Append("checked ");
                }
                Result.Append(">").Append(i.Label).Append("<Br>");
            }
            Result.Append("</p>\n");
            return Result.ToString();
        }
    }
    public class Item
    {
        public string Value { get; private set; }
        public string Label { get; private set; }
        public bool Checked { get; private set; }

        public Item(string value, string label, string check)
        {
            Value = value;
            Label = label;
            Checked = Convert.ToBoolean(check);
        }
    }

    public class Select : PageItem
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Class { get; set; }
        public bool Disabled { get; set; }
        public string[,] Options { get; set; }
        public List<Option> LOptions { get; private set; }

        public void FillList()
        {
            LOptions = new List<Option>();
            for (var i = 0; i < Options.Length / 3; i++)
            {
                LOptions.Add(new Option(Options[i, 0], Options[i, 1], Options[i, 2]));
            }
        }
        public override string ToHtmlStr()
        {
            var Result = new StringBuilder("<p><b>");
            Result.Append(Label).Append("</b><br></p>\n<p><select ");
            Result.AppendFormat("name = \"{0}\" value = \"{1}\" class = \"{2}\" ", Name, Value, Class);
            if (Required) Result.Append("Required ");
            if (Disabled) Result.Append("Disabled");
            Result.Append(">\n");
            if (LOptions == null || LOptions.Count == 0)
            {
                FillList();
            }
            foreach (var o in LOptions)
            {
                Result.Append("<option value =\"").Append(o.Value).Append("\" ");
                if (o.Selected)
                {
                    Result.Append("selected");
                }
                Result.Append(">").Append(o.Text).Append("</option>\n");
            }
            Result.Append("</select></p>\n");
            return Result.ToString();
        }
    }
    public class Option
    {
        public string Value { get; private set; }
        public string Text { get; private set; }
        public bool Selected { get; private set; }

        public Option(string value, string text, string selected)
        {
            Value = value;
            Text = text;
            Selected = Convert.ToBoolean(selected);
        }
    }

    public class Text : PageItem
    {
        public string Name { get; set; }
        public string Placeholder { get; set; }
        public bool Required { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Class { get; set; }
        public bool Disabled { get; set; }

        public override string ToHtmlStr()
        {
            var tmpRes = string.Format("<p><b>{0}</b><br>\n<input type = \"text\" name = \"{1}\" placeholder = \"{2}\" value = \"{3}\" class = \"{4}\" ", Label, Name, Placeholder, Value, Class);
            if (Required) tmpRes += "Required ";
            if (Disabled) tmpRes += "Disabled";
            tmpRes += ">\n </p>\n";
            return tmpRes;
        }
    }

    public class TextArea : PageItem
    {
        public string Name { get; set; }
        public string Placeholder { get; set; }
        public bool Required { get; set; }
        public string Value { get; set; }
        public string Label { get; set; }
        public string Class { get; set; }
        public bool Disabled { get; set; }

        public override string ToHtmlStr()
        {
            var tmpRes = string.Format("<p><b>{0}</b><br>\n<textarea type = \"text\" name = \"{1}\" placeholder = \"{2}\" value = \"{3}\" class = \"{4}\" ", Label, Name, Placeholder, Value, Class);
            if (Required) tmpRes += "Required ";
            if (Disabled) tmpRes += "Disabled";
            tmpRes += "></textarea>\n </p>\n";
            return tmpRes;
        }
    }
}
