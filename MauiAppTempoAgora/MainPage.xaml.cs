using MauiAppTempoAgora.Models;
using MauiAppTempoAgora.Services;

namespace MauiAppTempoAgora
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        public MainPage()
        {
            InitializeComponent();
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txt_cidade.Text))
                {
                    lbl_res.Text = "Preencha a cidade.";
                    return;
                }

                string cidade = txt_cidade.Text;
                string chave = "ce5ac41c46a5e251d991e398f400fb17";
                string url = $"https://api.openweathermap.org/data/2.5/weather?" +
                            $"q={cidade}&units=metric&lang=pt_br&appid={chave}";

                using (HttpClient cliente = new HttpClient())
                {
                    using (HttpResponseMessage response = await cliente.GetAsync(url))
                    {
                        // Verificação se a cidade não foi encontrada
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            await DisplayAlert("Cidade não encontrada",
                                $"A cidade '{cidade}' não foi encontrada. Verifique o nome e tente novamente.",
                                "OK");
                            return;
                        }

                        string json = await response.Content.ReadAsStringAsync();

                        // Usa o seu DataService existente
                        Tempo? t = await DataService.GetPrevisao(txt_cidade.Text);
                        if (t != null)
                        {
                            string dados_previsao = "";
                            dados_previsao = $"Latitude: {t.lat} \n" +
                                           $"Longitude: {t.lon} \n" +
                                           $"Nascer do Sol: {t.sunrise} \n" +
                                           $"Por do Sol: {t.sunset} \n" +
                                           $"Temp Máx: {t.temp_max} \n" +
                                           $"Temp Min: {t.temp_min} \n" +
                                           $"Como está o dia: {t.description} \n" +
                                           $"Velocidadade do vento: {t.speed} km/h \n" +
                                           $"Visibilidade: {t.visibility} km";
                            lbl_res.Text = dados_previsao;
                        }
                        else
                        {
                            lbl_res.Text = "Sem dados de Previsão";
                        }
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Verifica se é problema de internet
                if (ex.Message.Contains("No such host is known") ||
                    ex.Message.Contains("Name or service not known") ||
                    ex.Message.Contains("network is unreachable"))
                {
                    await DisplayAlert("Sem Internet",
                        "Você está sem conexão com a internet. Verifique sua conexão e tente novamente.",
                        "OK");
                }
                else
                {
                    await DisplayAlert("Erro de Conexão", $"Problema na conexão: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ops", ex.Message, "OK");
            }
        }
    }
}