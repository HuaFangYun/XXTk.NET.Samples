using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Smtp;
using System.Text;
using System.Threading.Tasks;

namespace XXTk.Consul.EmailSender
{
    public class NoticeEmailSender : INoticeEmailSender
    {
        public async Task SendAsync(IEnumerable<ConsulMessage> messages)
        {
            if (messages?.Any() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("服务发生故障，请及时处理，详细信息如下：");
            foreach (var message in messages)
            {
                sb.AppendLine($"Node: {message.Node}");
                sb.AppendLine($"ServiceID: {message.ServiceID}");
                sb.AppendLine($"ServiceName: {message.ServiceName}");
                sb.AppendLine($"CheckName: {message.Name}");
                sb.AppendLine($"CheckStatus: {message.Status}");
                sb.AppendLine($"CheckOutput: {message.Output}");
            }

            var mimeMessage = new MimeMessage();
            // 发送方（要求开通 SMTP服务）
            mimeMessage.From.Add(new MailboxAddress("MonitorQQMail", "发送方邮箱"));
            // 接收方
            mimeMessage.To.Add(new MailboxAddress("AdminMail", "接收方邮箱"));
            // 邮件主题
            mimeMessage.Subject = "Consul服务故障报警";
            // 邮件内容（HTML格式）
            mimeMessage.Body = new BodyBuilder() { HtmlBody = @$"<b>{sb}</b>" }.ToMessageBody();

            using var client = new SmtpClient();
            // 无需验证服务端凭据
            client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            await client.ConnectAsync("smtp.qq.com", 587, useSsl: false);
            // 移除OAuth2验证
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate("发送方邮箱", "开启SMTP服务时获取到授权码");

            await client.SendAsync(mimeMessage);
            await client.DisconnectAsync(quit: true);
        }
    }
}
