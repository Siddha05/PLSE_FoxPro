using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PLSE_FoxPro.Models
{
    public enum ValidationNumberType
    {
        MobilePhone,
        WorkPhone,
        Fax,
        PostCode,
        Expertise,
        Bill
    }
    public class GreaterThenAttribute : ValidationAttribute
    {
        public string PropertyName { get; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance,
            otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);

            if (((IComparable)value).CompareTo(otherValue) > 0)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("The current value is smaller than the other one");
        }
    }
    /// <summary>
    /// Аттрибут валидации номера, например мобильного, факса, экспертизы и т.д.
    /// <para>Тип номера задается в параметре конструктора</para>
    /// </summary>
    public class NumberAttribute : ValidationAttribute
    {
        public bool AllowEmpty { get; set; }
        public string Pattern { get; set; }
        public override bool IsValid(object value)
        {
            string val = value as string;
            if (AllowEmpty)
            {
                if (string.IsNullOrWhiteSpace(val)) return true;
                else return Regex.IsMatch(val, Pattern);
            }
            return Regex.IsMatch(val, Pattern);
        }
        public NumberAttribute(ValidationNumberType type)
        {
            Pattern = type switch
            {
                ValidationNumberType.MobilePhone => @"^[1-9]\d{9}$",
                ValidationNumberType.WorkPhone => @"^[1-9]\d{3,6}$",
                ValidationNumberType.Fax => @"^[1-9]\d{3,6}$",
                ValidationNumberType.PostCode => @"^[1-6][0-9]{5}$",//TODO: rework for complex postcode like 322122-322333
                ValidationNumberType.Expertise => @"^[1-9][0-9]{0,3}$",
                ValidationNumberType.Bill => @"^[1-9][0-9]{0,3}$",
                _ => throw new NotImplementedException(),
            };
            ErrorMessage = "неверный формат";
        }
    }
    /// <summary>
    /// Аттрибут валидации по регулярному выражению с поддержкой пустых строк
    /// </summary>
    public class RegularWithEmpty : ValidationAttribute
    {
        public bool AllowEmpty { get; set; }
        public int MaxLenght { get; set; } 
        public string Pattern { get; }
        public override bool IsValid(object value)
        {
            string res = value as string;
            if (string.IsNullOrWhiteSpace(res) || AllowEmpty) return true;
            else
            {
                return Regex.IsMatch(res, Pattern);
            }
        }
        public RegularWithEmpty(string pattern) => Pattern = pattern;
    }
    public class GraterOrEqualDateAttribute : ValidationAttribute
    {
        public string PropertyName { get; set; }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null) return new ValidationResult("неверная дата");
            object instance = validationContext.ObjectInstance,
            otherValue = instance.GetType().GetProperty(PropertyName).GetValue(instance);
            return otherValue switch
            {
                null => ValidationResult.Success,
                DateTime d when (DateTime)value >= d => ValidationResult.Success,
                _ => new ValidationResult("неверная дата")
            };
           
        }

    }
}