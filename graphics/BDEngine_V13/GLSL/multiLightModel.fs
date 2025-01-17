#version 330 core
out vec4 color;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 viewPos;
uniform sampler2D albedo;

struct DirLight{
	vec3 direction;
	
	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};
uniform DirLight dirLight;

struct PointLight{

	vec3 position;

	float constant;
	float linear;
	float quadratic;

	vec3 ambient;
	vec3 diffuse;
	vec3 specular;
};

uniform PointLight pLight[4];

vec3 CalcDirLight(DirLight light, vec3 normal, vec3 viewDir)
{
	vec3 lightDir = normalize(-light.direction);

	//diffuse shading
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff *light.diffuse * vec3(texture(albedo, TexCoord));

	//specular shading
	vec3 reflectDir = reflect(-lightDir, normal);
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 128);

	//combine results
	float ambientStrength = 0.1;
	vec3 ambient = ambientStrength * light.ambient ;
	
	vec3 specular = light.specular * spec * vec3(texture(albedo, TexCoord));
	
	return (ambient + diffuse + specular) ;
}
vec3 CalcPointLight(PointLight light, vec3 normal, vec3 fragPos, vec3 viewDir)
{
	vec3 lightDir = normalize(light.position - fragPos);
	//diffuse shading
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * light.diffuse * vec3(texture(albedo, TexCoord));
	//specular shading
	vec3 reflectDir = reflect(-lightDir, normal);	
	float spec = pow(max(dot(viewDir, reflectDir), 0.0), 128);

	//attenuation
	float distance = length(light.position - fragPos);
	float attenuation = 1.0 /(light.constant + light.linear*distance + light.quadratic * (distance * distance));
	//combine results
	float ambientStrength = 0.5;
	vec3 ambient = light.ambient * ambientStrength;
	
	vec3 specular = light.specular * spec * vec3(texture(albedo, TexCoord)) ;
	ambient *= attenuation;
	diffuse *= attenuation;
	specular *= attenuation;

	return (ambient + diffuse + specular);
}
void main()
{
	//properties
	vec3 norm = normalize(Normal);
	vec3 viewDir = normalize(viewPos - FragPos);
	vec3 result = vec3(0.0f, 0.0f, 0.0f);

	//phase 1: Directional lighting
	result = CalcDirLight(dirLight, norm, viewDir);
	
	//phase 2: Point lights
	for(int i =0; i<4; i++)
		result += CalcPointLight(pLight[i], norm, FragPos, viewDir);
	
	result = result * vec3(texture(albedo, TexCoord));	
	color = vec4(result, 1.0);
}