using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace TransactionManager
{
    class TransactionProcessor
    {
        private const string ADD_COMMAND = "add";
        private const string GET_COMMAND = "get";

        private Transaction _newTransaction;
        private State _state;
        private Dictionary<int, Transaction> transactions = new Dictionary<int, Transaction>();

        public TransactionProcessor()
        {
            _state = State.Idle;
        }

        public ProcessResult Process(string input)
        {
            ProcessResult result = null;
            var resultMessage = string.Empty;

            switch (_state)
            {
                case State.Idle:
                    if (string.Equals(input, ADD_COMMAND, StringComparison.OrdinalIgnoreCase))
                    {
                        _state = State.Add_Id;
                        _newTransaction = new Transaction();
                        result = new ProcessResult { Message = "Введите Id: " };
                    }
                    else if (string.Equals(input, GET_COMMAND, StringComparison.OrdinalIgnoreCase))
                    {
                        _state = State.Get;
                        result = new ProcessResult { Message = "Введите Id: " };
                    }
                    else
                    {
                        result = new ProcessResult { Message = "Неизвестная команда" + Environment.NewLine };
                    }      

                    break;
                case State.Add_Id:
                    if (TryParseId(input, out int id, out resultMessage))
                    {
                        if (!transactions.ContainsKey(id))
                        {
                            _state = State.Add_Date;
                            _newTransaction.Id = id;
                            result = new ProcessResult() { Message = "Введите дату: " };
                            break;
                        }
                        else
                        {
                           resultMessage =  "Ошибка: Информация о транзакции с таким id уже существует";
                        }
                    }

                    resultMessage += Environment.NewLine + "Введите Id: ";
                    result = new ProcessResult() { Message = resultMessage };

                    break;
                case State.Add_Date:
                    if (TryParseDate(input, out DateTime date, out resultMessage))
                    {
                        _state = State.Add_Amount;
                        _newTransaction.TransactionDate = date;
                        result = new ProcessResult { Message = "Введите сумму: " };
                        break;
                    }

                    resultMessage += Environment.NewLine + "Введите дату: ";
                    result = new ProcessResult() { Message = resultMessage };

                    break;
                case State.Add_Amount:
                    if (TryParseAmount(input, out decimal amount, out resultMessage))
                    {
                        _state = State.Idle;
                        _newTransaction.Amount = amount;
                        transactions.Add(_newTransaction.Id, _newTransaction);
                        result = new ProcessResult { Message = "[OK]" + Environment.NewLine };
                        break;
                    }

                    resultMessage += Environment.NewLine + "Введите сумму: ";
                    result = new ProcessResult() { Message = resultMessage };

                    break;
                case State.Get:
                    if (TryParseId(input, out int correctId, out resultMessage))
                    {
                        if (transactions.ContainsKey(correctId))
                        {
                            _state = State.Idle;
                            var existingTransaction = transactions[correctId];
                            var serializedTransaction = SerializeTransaction(existingTransaction);

                            resultMessage = serializedTransaction +
                                            Environment.NewLine +
                                            "[OK]" +
                                            Environment.NewLine;

                            result = new ProcessResult() { Message = resultMessage };
                            break;
                        }
                        else
                        {
                            resultMessage = "Ошибка: Нет данных о транзакции с указанным id";
                        }
                    }

                    resultMessage += Environment.NewLine + "Введите Id: ";
                    result = new ProcessResult() { Message = resultMessage };
                    break;
            }

            return result;
        }

        private bool TryParseId(string inputId, out int id, out string errMessage)
        {
            errMessage = "";

            if (!int.TryParse(inputId, out id))
            {
                errMessage = "Ошибка: Id должен быть целым числом";
                return false;
            }

            if (id <= 0)
            {
                errMessage = "Ошибка: Id должен быть больше 0";
                return false;
            }

            return true;
        }

        private bool TryParseDate(string inputDate, out DateTime date, out string errMessage)
        {
            errMessage = "";

            if (!DateTime.TryParseExact(inputDate,
                            "dd.MM.yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out date))
            {
                errMessage = "Ошибка: Неверный формат даты. Используйте формат \"dd.MM.yyyy\"";
                return false;
            }

            if (date > DateTime.Now)
            {
                errMessage = "Ошибка: Дата транзакции не может быть больше текущей даты";
                return false;
            }

            return true;
        }

        private bool TryParseAmount(string inputAmount, out decimal amount, out string errMessage)
        {
            errMessage = "";
            amount = 0.00m;

            Regex decimalMatch = new Regex(@"^-?\d+\.\d{2}$");

            if (!decimalMatch.IsMatch(inputAmount))
            {
                errMessage = "Ошибка: Неверный формат суммы. Используйте формат #.##";
                return false;
            }
            else
            {
                amount = decimal.Parse(inputAmount, CultureInfo.InvariantCulture);
            }

            return true;
        }

        private string SerializeTransaction(Transaction transaction)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            return JsonSerializer.Serialize(transaction, options);
        }

        enum State
        {
            Idle,
            Add_Id,
            Add_Date,
            Add_Amount,
            Get
        }
    }
}