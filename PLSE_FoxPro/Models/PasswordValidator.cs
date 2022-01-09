using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PLSE_FoxPro.Models
{
    public class PasswordValidator : ObservableObject
    {
        #region Fields
        readonly bool _digit_required;
        readonly bool _upper_required;
        readonly int _min_lenght;
        readonly Func<char, bool> _valid_char;

        bool _has_digit;
        bool _has_upper;
        bool _has_all_valid;
        bool _has_min_lenght;
        bool _equals;
        private string _pass;
        private string _pass_repeat;
        #endregion

        #region Properties
        public bool IsValidState => IsValid();
        public bool HasDigit 
        {
            get => _has_digit;
            set => SetProperty(ref _has_digit, value); 
        }
        public bool HasUpperLetter
        { 
            get => _has_upper;
            set => SetProperty(ref _has_upper, value);
        }
        public bool IsAllValidCharacters
        { 
            get => _has_all_valid;
            set => SetProperty(ref _has_all_valid, value);
        }
        public bool HasMinLenght 
        { 
            get => _has_min_lenght;
            set => SetProperty(ref _has_min_lenght, value); 
        }
        public string Password
        {
            get => _pass;
            set 
            {
                SetProperty(ref _pass, value);
                SetState();
            }
        }
        public string PasswordRepeat
        {
            get => _pass_repeat;
            set
            {
                SetProperty(ref _pass_repeat, value);
                IsEquals = _pass.Equals(_pass_repeat, StringComparison.Ordinal);
            }
        }
        public bool IsEquals
        {
            get => _equals;
            set => SetProperty(ref _equals, value);
        }
        #endregion

        #region Functions
        private void SetState()
        {
            if (string.IsNullOrWhiteSpace(_pass))
            {
                HasDigit = _digit_required ? false : true;
                HasMinLenght = false;
                HasUpperLetter = _upper_required ? false : true;
                IsAllValidCharacters = false;
            }
            bool _has_invalid = false;
            foreach (var ch in _pass)
            {
                if (Char.IsNumber(ch)) HasDigit = true;
                if (Char.IsUpper(ch)) HasUpperLetter = true;
                if (!_valid_char(ch)) _has_invalid = true;
            }
            IsAllValidCharacters = _has_invalid ? false : true;
            HasMinLenght = _pass.Length >= _min_lenght;
            IsEquals = _pass.Equals(_pass_repeat, StringComparison.Ordinal);
        }
        public bool IsValid()
        {
            //return _digit_required ? _has_digit : true 
            //        && _upper_required ? _has_upper : true
            //        && _has_all_valid 
            //        && _equals 
            //        && _has_min_lenght;
            return _has_min_lenght;
        }
        #endregion
        /// <summary>
        /// Создает экземпляр класса c начальными настройками.
        /// </summary>
        /// <remarks>Если в <paramref name="charValidator"/> передан null, допустимымы считаются только цифры и буквы</remarks>
        /// <param name="digitRequired">Необходима ли в пароле цифра </param>
        /// <param name="upperRequired">Необходима ли в пароле буква в верхнем регистрк</param>
        /// <param name="minLenght">Минимальная необходимая длинна пароля</param>
        /// <param name="charValidator">Предикат допустимости символов пароля</param>
        public PasswordValidator(bool digitRequired = true, bool upperRequired = true, int minLenght = 8, Func<char, bool> charValidator = null)
        {
            _digit_required = digitRequired;
            _upper_required = upperRequired;
            _min_lenght = minLenght;
            _valid_char = charValidator ?? new Func<char, bool>((n) => Char.IsLetterOrDigit(n));
        }
    }
}
