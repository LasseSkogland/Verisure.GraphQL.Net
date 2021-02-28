using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Verisure.GraphQL.Data {
    public class GraphQLQuery {
        public string OperationName { get; set; }
        public string Query { get; set; }
        public IDictionary<string, dynamic> Variables { get; set; } = new Dictionary<string, dynamic>() { };

        public Dictionary<string, dynamic> ToDictionary() {
            return new Dictionary<string, dynamic> {
                ["operationName"] = OperationName,
                ["query"] = Query,
                ["variables"] = Variables,
            };
        }

        private static string ParseObject(NewExpression body, int indentation = 0) {
            StringBuilder sb = new StringBuilder();
            string objectParams = null;
            for (int i = 0; i < body.Members.Count; i++) {
                var memberName = body.Members[i].Name;
                switch (body.Arguments[i].NodeType) {
                    case ExpressionType.Constant:
                        if (memberName == "_params") {
                            objectParams = (body.Arguments[i] as ConstantExpression).Value as string;
                        }
                        break;
                    case ExpressionType.NewArrayInit:
                        var newArray = (body.Arguments[i] as NewArrayExpression);
                        sb.Indent(indentation).Append(memberName).Append(" {\n");
                        foreach (var item in newArray.Expressions) {
                            Console.WriteLine();
                            sb.Indent(indentation + 1).Append((item as ConstantExpression).Value as string).Append("\n");
                        }
                        sb.Indent(indentation).Append("}\n");
                        break;
                    case ExpressionType.New:
                        var parsed = ParseObject(body.Arguments[i] as NewExpression, indentation + 1);

                        sb.Indent(indentation).Append(memberName);
                        if (objectParams != null) {
                            sb.AppendFormat("({0})", objectParams);
                        }
                        sb.Append(" {").Append("\n")
                            .Append(parsed)
                            .Indent(indentation).Append("}").Append("\n");
                        break;
                }
            }
            return sb.ToString();
        }

        public static string CreateQueryString(string type, Expression<Func<dynamic>> exp) {
            if (exp.Body.NodeType == ExpressionType.New) {
                StringBuilder sb = new StringBuilder();
                sb.Append(type).Append(' ');
                if (exp.Body.NodeType == ExpressionType.New) {
                    var body = exp.Body as NewExpression;
                    var parsed = ParseObject(body);
                    sb.Append(parsed);
                }
                else {
                    throw new Exception("Invalid object");
                }
                return sb.ToString();
            }
            return default;
        }

        private static IDictionary<string, dynamic> CreateDictionary(NewExpression exp) {
            Dictionary<string, dynamic> output = new Dictionary<string, dynamic>();
            for (int i = 0; i < exp.Members.Count; i++) {
                if (exp.Arguments[i].NodeType == ExpressionType.Constant) {
                    output.Add(exp.Members[i].Name, (exp.Arguments[i] as ConstantExpression).Value);
                }
                else if (exp.Arguments[i].NodeType == ExpressionType.New) {

                    output.Add(exp.Members[i].Name, CreateDictionary(exp.Arguments[i] as NewExpression));
                }
            }
            return output;
        }

        public static IDictionary<string, dynamic> CreateVariables(Expression<Func<dynamic>> exp) {
            if (exp.Body.NodeType == ExpressionType.New) {
                return CreateDictionary(exp.Body as NewExpression);
            }
            return default;
        }
    }
}
