//using System;
using System.Collections.Generic;
using System.Text;
using PLSE_FoxPro.Models;

namespace PLSE_FoxPro.DesignData
{
    public static class TestInstance
    {
        public static Speciality SpecialityShort => new Speciality(id: -2, code: "10.2", title: "Исследование лакокрасочных материалов и покрытий", species: "криминалистическая экспертиза материалов, веществ и изделий",
                                                                   cat_1: 22, cat_2: 44, cat_3: 115, acr: "ЛКМиП", isvalid: true, vr: Version.Original, updatedate: new System.DateTime(2001, 11, 5));
        public static Speciality SpecialityMedium => new Speciality(id: -1, code: "13.3", title: " Исследование следов на транспортных средствах и месте дорожно-транспортного происшествия (транспортно-трасологическая диагностика)", species: "автотехническая экспертиза",
                                                                    cat_1: 17, cat_2: 38, cat_3: 85, acr: null, isvalid: true, vr: Version.Original, updatedate: new System.DateTime(2010, 12, 15));
        public static Speciality SpecialityLong => new Speciality(id: -3, code: "10.5", title: "Исследование наркотических средств, психотропных веществ и их прекурсоров, сильдействующих и ядовитых веществ, лекарственных средств", species: "криминалистическая экспертиза материалов, веществ и изделий",
                                                               cat_1: 11, cat_2: 56, cat_3: 115, acr: null, isvalid: false, vr: Version.Original, updatedate: new System.DateTime(2011, 12, 15));
        public static Employee EmployeeKojaeva => new Employee(id: 7, departament: DepartamentKE, office: "ведущий государственный судебный эксперт", firstname: "Ольга", middlename: "Юрьевна",
                     secondname: "Кожаева", password: "password", emplstatus: "работает", foto: null,
                     gender: false, declinated: true, profile: PermissionProfile.Expert, vr: Version.Original, updatedate: System.DateTime.Now)
        ;
        private static Employee_SlightPart EmployeeKojaeve_Slight => new Employee_SlightPart(hiredate: new System.DateTime(2008, 8, 23), birthdate: new System.DateTime(1982, 10, 14),
            mobilephone: "9875031438", workphone: "683763", email: "Kojaeva82@mail.ru", adress: AdressPenza1, education1: "высшее педагогическое образование по специальности 'История'",
            education2: "высшее юридическое образование по специальности 'Юриспруденция'", education3: null, sciencedegree: null, hidepersonal: true);
        private static Employee_SlightPart EmployeeKozlova_Slight => new Employee_SlightPart(hiredate: new System.DateTime(2016, 6, 1), birthdate: new System.DateTime(1991, 11, 6),
            mobilephone: null, workphone: "686126", email: null, adress: null, education1: "высшее образование по специальности \"Биохимия\", квалификацию магистра по направлению подготовки \"Биология\" (профиль \"Биохимия и молекулярная биология\")",
            education2: null, education3: null, sciencedegree: null, hidepersonal: false);
        public static Employee EmployeeKozlova => new Employee(id: 42, departament: DepartamentKE, office: "государственный судебный эксперт", firstname: "Галина", middlename: "Александровна",
                     secondname: "Козлова", password: "password123", emplstatus: "работает", foto: null,
                     gender: false, declinated: true, profile: PermissionProfile.Expert, vr: Version.Original, updatedate: System.DateTime.Now)
        ;
        public static Departament DepartamentKE => new Departament(id: 0, title: "отдел криминалистических экспертиз", acronym: "КЭ", code: "3", isvalid: true, vr: Version.Original);
        public static Settlement Penza => new Settlement(id: -1, title: "Пенза", type: "г.", significance: "федеральный", telephonecode: "8412", postcode: "440000",
                                federallocation: "Пензенская область", territoriallocation: null, isvalid: true, vr: Version.Original, updatedate: System.DateTime.Now);
        public static Adress AdressPenza1 => new Adress()
        {
            Settlement = Penza,
            Housing = "146",
            Flat = "299",
            StreetPrefix = "ул.",
            Street = "Ладожская"
        };
        public static Adress AdressBashmakovo => new Adress()
        {
            Settlement = new Settlement(id: -3, title: "Башмаково", type: "рп", significance: "районный", telephonecode: "+784143", postcode: "442060",
                                federallocation: "Пензенская область", territoriallocation: "Башмаковский р-н", isvalid: true, vr: Version.Original, updatedate: System.DateTime.Now),
            Housing = "8",
            StreetPrefix = "ул.",
            Street = "Плеханова"
        };
        public static Adress AdressBessonovka => new Adress()
        {
            Settlement = new Settlement(id: -4, title: "Бессоновка", type: "с.", significance: "районный", telephonecode: "+784140", postcode: "442780",
                               federallocation: "Пензенская область", territoriallocation: "Бессоновский р-н", isvalid: true, vr: Version.Original, updatedate: System.DateTime.Now),
            Housing = "227",
            StreetPrefix = "ул.",
            Street = "Центральная"
        };
        public static Expert ExpertKojaeva1 => new Expert(id: 0, speciality: SpecialityShort, receiptdate: new System.DateTime(2014, 3, 21), lastattestationdate: new System.DateTime(2019, 2, 13),
                                            employee: EmployeeKojaeva, updatedate: new System.DateTime(2014, 3, 22), vr: Version.Original);
        public static Expert ExpertKojaeva2 => new Expert(id: 0, speciality: SpecialityLong, receiptdate: new System.DateTime(2012, 5, 11), lastattestationdate: new System.DateTime(2017, 3, 21),
                                            employee: EmployeeKojaeva, updatedate: new System.DateTime(2012, 5, 11), vr: Version.Original, closed: true);
        public static Expert ExpertKojaeva3 => new Expert(id: 0, speciality: SpecialityMedium, receiptdate: new System.DateTime(2015, 7, 21), lastattestationdate: null,
                                            employee: EmployeeKojaeva, updatedate: new System.DateTime(2019, 3, 12), vr: Version.Original);
        public static Organization OrganizationSud => new Organization(id: -1, name: "Башмаковский районный суд Пензенской области", shortname: "Башмаковский р/суд Пензенской области",
                                                                        postcode: "442060", adress: AdressBashmakovo, telephone: "41270", telephone2: null,
                                                                        fax: "4-12-98", email: "bashmakovsky.pnz@sudrf.ru", website: "bashmakovsky.ru", status: true,
                                                                        vr: Version.Original, updatedate: new System.DateTime(2015, 5, 22));
        public static Organization OrganizationOMVD => new Organization(id: -2, name: "Отделение МВД РФ по Бессоновскому району Пензенской области", shortname: "ОМВД РФ по Бессоновскому району Пензенской области",
                                                                        postcode: "442780", adress: AdressBessonovka, telephone: "25202", telephone2: "25203",
                                                                        fax: "25204", email: null, website: null, status: true,
                                                                        vr: Version.Original, updatedate: new System.DateTime(2017, 11, 4));
        public static Customer CustomerJudje => new Customer(firstname: "Наталья", secondname: "Коваленко", middlename: "Олеговна", mobilephone: "9992451245", workphone: "41275", gender: false,
                                        email: "Kovalenko89@mail.ru", declinated: true, vr: Version.Original, updatedate: new System.DateTime(2018, 12, 4), id: -1, previd: null, rank: null, office: "судья",
                                        organization: OrganizationSud, departament: null, status: true);
        public static Customer CustomerInvestigator => new Customer(firstname: "А", secondname: "Москвин", middlename: "С", mobilephone: "9781326489", workphone: null, gender: true,
                                        email: null, declinated: true, vr: Version.Original, updatedate: new System.DateTime(2021, 5, 14), id: -2, previd: null, rank: "старший лейтенант", office: "старший дознаватель",
                                        organization: OrganizationOMVD, departament: "ОД", status: true);
        public static Resolution Resolution1 => new Resolution(id: -1, registrationdate: new System.DateTime(2020, 12, 15), resolutiondate: new System.DateTime(2020, 12, 13),
                                resolutiontype: "определение", customer: CustomerJudje, prescribe: "комплексная судебная экспертиза",
                                obj: "материалы гражданского дела № 2-1107/2018 (1 том на 158 л.)|договор займа от 12.08.2017 на 1л.|договор залога от 12.08.2017 на 1л.",
                                quest: "Соответствует ли дата выполнения документов - договора залога и договора найма от 12.08.2017 (ШААНКСИ-У403СА62) дате, указанной на документе?",
                                nativenumeration: true, status: "", casenumber: "2-1453/2020", respondent: "Грылев Олег Сергеевич, Кулаков Александр Валерьевич", plaintiff: "Кулакова Татьяна Александровна",
                                typecase: new CaseType("1", "гражданское"),
                                uidcase: "3232-FE1223322",
                                annotate: "по иску Кулаковой Татьяны Александровны к Грылеву Олегу Сергеевичу и Кулакову Александру Валерьевичу о признании сделки недействительной",
                                comment: "комментарий составлен невпопад и незачем, но надо", vr: Version.Original);
        public static Request Request1 => new Request(-1, Expertise2, new System.DateTime(2020, 12, 19), new System.DateTime(2020, 12, 19), "осмотр",true, Version.Original);
        public static Response Response1 => new Response(-2, Expertise2, new System.DateTime(2020, 1, 9), new System.DateTime(2020, 1, 9),"осмотр",-1, true, Version.Original);
        public static Expertise Expertise1 => new Expertise(id: -1, number: "3", expert: ExpertKojaeva1, result: "заключение", start: new System.DateTime(2020, 11, 8),
                        end: new System.DateTime(2021, 3, 13), timelimit: 30, type: "первичная", previous: 12, spendhours: 48, vr: Version.Original)
        //{ Evaluation = 7 }
        ;
        public static Expertise Expertise2
        {
            get
            {
                var e = new Expertise(id: -1, number: "3245", expert: ExpertKojaeva2, result: "заключение", start: new System.DateTime(2020, 12, 18),
                        end: new System.DateTime(2021, 1, 23), timelimit: 30, type: "дополнительная", previous: null, spendhours: 48, vr: Version.Original)
                //{ Evaluation = 4, }
                ;
                e.Bills.Add(Bill2); e.Bills.Add(Bill3);
                e.Movements.Add(Request1); e.Movements.Add(Response1);
                e.Movements.Add(Report1);
                return e;
            }
        }
        public static Bill Bill1 => new Bill(-1, Expertise1, "407", new System.DateTime(2021, 03, 21), null, "истец", 32, 760.00m, 0m, Version.Original);
        public static Bill Bill2 => new Bill(-1, Expertise2, "1206", new System.DateTime(2020, 11, 1), new System.DateTime(2020, 11, 6), "истец", 16, 660.00m, 10560m, Version.Original);
        public static Bill Bill3 => new Bill(-1, Expertise2, "1207", new System.DateTime(2020, 11, 1), new System.DateTime(2020, 11, 6), "ответчик", 16, 660.00m, 5300m, Version.Original);
        public static Report Report1 => new Report(-1, Expertise1, new System.DateTime(2021, 1, 22), new System.DateTime(2021, 1, 22),"высокая загруженность", new System.DateTime(2021, 3, 31), Version.Original);
        public static Equipment EquipmentMicr => new Equipment(id: -1, name: "Leiсa M 165 С",
            description: "Микроскоп стереоскопический «Leiсa M 165 С» c цифровой камерой Leica DFC450 и программным обеспечением Leica Application Suite",
            commisiondate: new System.DateTime(2013, 01, 25), check: null, istrack: true, updatedate: new System.DateTime(2014, 11, 4), vr: Version.Original);
        public static Equipment EquipmentConst => new Equipment(id: -2, name: "Константа К5",
            description: "Прибор измерений геометрических параметров и параметров окружающей среды многофункциональный «Константа К5»",
            commisiondate: new System.DateTime(2020, 9, 5), check: null, istrack: true, updatedate: new System.DateTime(2020, 9, 8), vr: Version.Original);
        public static EquipmentUsage UsageMicr => new EquipmentUsage(id: -1, Expertise2, new System.DateTime(2020, 11, 14), 2, EquipmentMicr, Version.Original); 
        
        public static EquipmentUsage UsageConst => new EquipmentUsage (-2, Expertise1, new System.DateTime(2020, 11, 20), 1, EquipmentConst, Version.Original);
        
    }
}
