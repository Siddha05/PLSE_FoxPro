using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PLSE_FoxPro.Models
{
    public abstract class ExpertiseMovement : VersionBase
    {
        #region Fields
        DateTime? _create;
        DateTime _register;
        #endregion
        #region Properties
        public virtual DateTime? CreationDate 
        { 
            get => _create; 
            set => SetProperty(ref _create, value);
        }
        public DateTime RegistrationDate
        { 
            get => _register; 
            set => SetProperty(ref _register, value);
        }
        #endregion

        public ExpertiseMovement(int id, DateTime? create, DateTime register, Version version) : base(id, version)
        {
            _create = create;
            _register = register;
        }
    }

    public sealed class Response : ExpertiseMovement
    {
        #region Fields
        string _content;
        int _refer_to;
        bool _resume;
        #endregion

        #region Properties
        [Required(ErrorMessage = "обязательное поле")][MaxLength(1000, ErrorMessage = "превышен лимит символов")]
        public string Content
        { 
            get => _content;
            set => SetProperty(ref _content, value, true );
        }
        public int ReferTo
        {
            get => _refer_to;
            set => SetProperty(ref _refer_to, value);
        }
        public bool ResumeExecution
        {
            get => _resume; 
            set => SetProperty(ref _resume, value);
        }
        #endregion

        public Response(int id, DateTime? create, DateTime register, string content, int refer, bool resume, Version version) 
            : base(id, create, register, version)
        {
            _content = content;
            _refer_to = refer;
            _resume = resume;
        }
    }

    public sealed class Request : ExpertiseMovement
    {
        #region Fields
        string _content;
        bool _suspend;
        #endregion

        #region Properties
        [Required(ErrorMessage = "обязательное поле")][MaxLength(1000, ErrorMessage = "превышен лимит символов")]
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value, true);
        }
        public bool SuspendExecution
        {
            get => _suspend;
            set => SetProperty(ref _suspend, value);
        }
        #endregion
        public Request(int id, DateTime? create, DateTime register, string content, bool suspend, Version version)
            : base(id, create, register, version)
        {
            _content = content;
            _suspend = suspend;
        }
    }

    public sealed class Report : ExpertiseMovement
    {
        #region Fields
        DateTime _delay;
        string _reason;
        #endregion

        #region Properties
        /// <summary>
        /// До какой даты продлена экспертиза
        /// </summary>
        public DateTime DelayDate 
        {
            get => _delay;
            set => SetProperty(ref _delay, value);
        }
        /// <summary>
        /// Причина продления экспертизы
        /// </summary>
        [Required(ErrorMessage = "обязательное поле")][MaxLength(1000, ErrorMessage = "превышен лимит символов")]
        public string Reason 
        {
            get => _reason;
            set => SetProperty(ref _reason, value, true);
        }
        #endregion

        public Report(int id, DateTime? create, DateTime register, string reason, DateTime delay, Version version)
            : base(id, create, register, version)
        {
            _delay = delay;
            _reason = reason;
        }
    }

    public sealed class IncomingLetter : ExpertiseMovement //TODO: implement
    {
        public IncomingLetter(int id, DateTime? create, DateTime register, Version version) : base(id, create, register, version)
        {

        }
    }

    public class OutcomingLetter : ExpertiseMovement //TODO: implement
    {
        public OutcomingLetter(int id, DateTime? create, DateTime register, Version version) : base(id, create, register, version)
        {

        }
    }
}
